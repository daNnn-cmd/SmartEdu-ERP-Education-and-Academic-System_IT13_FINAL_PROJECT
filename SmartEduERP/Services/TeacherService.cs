using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartEduERP.Services;

public class TeacherService
{
    private readonly SmartEduDbContext _context;
    private readonly AuditLogService _auditLogService;
    private readonly ISyncQueueService _syncQueueService;

    public TeacherService(SmartEduDbContext context, AuditLogService auditLogService, ISyncQueueService syncQueueService)
    {
        _context = context;
        _auditLogService = auditLogService;
        _syncQueueService = syncQueueService;
    }

    public async Task<List<Teacher>> GetAllTeachersAsync()
    {
        return await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public async Task<Teacher?> GetTeacherByIdAsync(int id)
    {
        return await _context.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Grades)
            .FirstOrDefaultAsync(t => t.TeacherId == id);
    }

    // Validates the Teacher model using DataAnnotations
    private static void ValidateTeacherModel(Teacher teacher)
    {
        // Centralized sanitization + DataAnnotations validation
        ValidationHelper.SanitizeAndValidateModel(teacher);
    }

    public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
    {
        try
        {
            Console.WriteLine("🔍 TeacherService.CreateTeacherAsync() called");
            Console.WriteLine($"📝 Creating teacher: {teacher.FirstName} {teacher.LastName} ({teacher.Email})");

            // Run full DataAnnotations validation (enforces required fields, regex, etc.)
            ValidateTeacherModel(teacher);

            // Check if email already exists
            var existingTeacher = await _context.Teachers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Email == teacher.Email);

            if (existingTeacher != null)
                throw new InvalidOperationException($"A teacher with email '{teacher.Email}' already exists");

            // Set default values if not provided
            teacher.IsDeleted = false;
            teacher.Position = string.IsNullOrWhiteSpace(teacher.Position) ? "Teacher" : teacher.Position;

            // Ensure registration date is set (default to current date if not provided)
            if (teacher.RegistrationDate == default)
                teacher.RegistrationDate = DateTime.UtcNow;

            // Ensure timestamps are initialized so sync can use UpdatedAt for conflict resolution
            var now = DateTime.UtcNow;
            teacher.CreatedAt = now;
            teacher.UpdatedAt = now;

            // Add teacher to context and save to get the TeacherId
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Teacher created successfully with ID: {teacher.TeacherId}");

            try
            {
                await _syncQueueService.QueueOperationAsync(
                    "Create",
                    "Teachers",
                    teacher.TeacherId,
                    new
                    {
                        teacher.TeacherId,
                        teacher.FirstName,
                        teacher.LastName,
                        teacher.Email,
                        teacher.Department,
                        teacher.Position,
                        teacher.RegistrationDate
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error queueing teacher create sync: {ex.Message}");
            }

            // Log teacher creation
            try
            {
                var newTeacherJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    teacher.TeacherId,
                    teacher.FirstName,
                    teacher.LastName,
                    teacher.Email,
                    teacher.Department,
                    teacher.Position,
                    teacher.RegistrationDate
                });

                await _auditLogService.LogActionAsync(
                    action: "Create",
                    tableName: "Teachers",
                    recordId: teacher.TeacherId,
                    userId: null,
                    oldValues: null,
                    newValues: newTeacherJson
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging audit: {ex.Message}");
            }

            return teacher;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR in CreateTeacherAsync: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<Teacher?> UpdateTeacherAsync(Teacher teacher)
    {
        var existing = await _context.Teachers.FindAsync(teacher.TeacherId);
        if (existing == null)
            return null;

        // Run full DataAnnotations validation on the incoming model
        ValidateTeacherModel(teacher);

        if (string.IsNullOrWhiteSpace(teacher.FirstName))
            throw new ArgumentException("First name is required");
        if (string.IsNullOrWhiteSpace(teacher.LastName))
            throw new ArgumentException("Last name is required");
        if (string.IsNullOrWhiteSpace(teacher.Email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(teacher.Department))
            throw new ArgumentException("Department is required");

        var otherTeacherWithEmail = await _context.Teachers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Email == teacher.Email && t.TeacherId != teacher.TeacherId);

        if (otherTeacherWithEmail != null)
            throw new InvalidOperationException($"A teacher with email '{teacher.Email}' already exists");

        var oldTeacherJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            existing.TeacherId,
            existing.FirstName,
            existing.LastName,
            existing.Email,
            existing.Department,
            existing.Position,
            existing.RegistrationDate
        });

        existing.FirstName = teacher.FirstName;
        existing.LastName = teacher.LastName;
        existing.Email = teacher.Email;
        existing.Department = teacher.Department;
        existing.Position = teacher.Position;
        existing.RegistrationDate = teacher.RegistrationDate;
        existing.UpdatedAt = DateTime.UtcNow;

        var linkedUsers = await _context.UserAccounts
            .IgnoreQueryFilters()
            .Where(u => u.ReferenceId == existing.TeacherId &&
                        u.Role != null &&
                        u.Role.ToLower() == "teacher")
            .ToListAsync();

        foreach (var user in linkedUsers)
        {
            user.FirstName = existing.FirstName;
            user.LastName = existing.LastName;
            user.Email = existing.Email;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Teachers",
                existing.TeacherId,
                new
                {
                    existing.TeacherId,
                    existing.FirstName,
                    existing.LastName,
                    existing.Email,
                    existing.Department,
                    existing.Position,
                    existing.RegistrationDate
                },
                new
                {
                    existing.TeacherId,
                    existing.FirstName,
                    existing.LastName,
                    existing.Email,
                    existing.Department,
                    existing.Position,
                    existing.RegistrationDate
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing teacher update sync: {ex.Message}");
        }

        try
        {
            var newTeacherJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                existing.TeacherId,
                existing.FirstName,
                existing.LastName,
                existing.Email,
                existing.Department,
                existing.Position,
                existing.RegistrationDate
            });

            await _auditLogService.LogActionAsync(
                action: "Update",
                tableName: "Teachers",
                recordId: teacher.TeacherId,
                userId: null,
                oldValues: oldTeacherJson,
                newValues: newTeacherJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging audit: {ex.Message}");
        }

        return existing;
    }

    public async Task<bool> SoftDeleteTeacherAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
            return false;

        var deletedTeacherJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            teacher.TeacherId,
            teacher.FirstName,
            teacher.LastName,
            teacher.Email,
            teacher.Department
        });

        teacher.IsDeleted = true;
        teacher.DeletedAt = DateTime.UtcNow;
        teacher.UpdatedAt = DateTime.UtcNow;
        teacher.UpdatedAt = DateTime.UtcNow;

        var linkedUsers = await _context.UserAccounts
            .IgnoreQueryFilters()
            .Where(u => u.ReferenceId == teacher.TeacherId &&
                        u.Role != null &&
                        u.Role.ToLower() == "teacher")
            .ToListAsync();

        foreach (var user in linkedUsers)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Delete",
                "Teachers",
                teacher.TeacherId,
                new
                {
                    teacher.TeacherId,
                    teacher.FirstName,
                    teacher.LastName,
                    teacher.Email,
                    teacher.Department
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing teacher delete sync: {ex.Message}");
        }

        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Teachers",
                recordId: id,
                userId: null,
                oldValues: deletedTeacherJson,
                newValues: "Teacher soft deleted"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging audit: {ex.Message}");
        }

        return true;
    }

    // Overload that captures an archive reason and logs it (no table changes)
    public async Task<bool> SoftDeleteTeacherAsync(int id, string reason)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
            return false;

        var trimmedReason = (reason ?? string.Empty).Trim();

        var deletedTeacherJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            teacher.TeacherId,
            teacher.FirstName,
            teacher.LastName,
            teacher.Email,
            teacher.Department,
            Reason = trimmedReason
        });

        teacher.IsDeleted = true;
        teacher.DeletedAt = DateTime.UtcNow;
        teacher.UpdatedAt = DateTime.UtcNow;

        var linkedUsers = await _context.UserAccounts
            .IgnoreQueryFilters()
            .Where(u => u.ReferenceId == teacher.TeacherId &&
                        u.Role != null &&
                        u.Role.ToLower() == "teacher")
            .ToListAsync();

        foreach (var user in linkedUsers)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Delete",
                "Teachers",
                teacher.TeacherId,
                new
                {
                    teacher.TeacherId,
                    teacher.FirstName,
                    teacher.LastName,
                    teacher.Email,
                    teacher.Department,
                    Reason = trimmedReason
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing teacher delete sync: {ex.Message}");
        }

        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Teachers",
                recordId: id,
                userId: null,
                oldValues: deletedTeacherJson,
                newValues: string.IsNullOrWhiteSpace(trimmedReason) ? "Teacher soft deleted" : $"Teacher soft deleted. Reason: {trimmedReason}"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging audit: {ex.Message}");
        }

        return true;
    }

    // Search teachers by common fields
    public async Task<List<Teacher>> SearchTeachersAsync(string searchTerm)
    {
        var lowerSearch = searchTerm.ToLower();
        return await _context.Teachers
            .AsNoTracking()
            .Where(t => (t.FirstName ?? string.Empty).ToLower().Contains(lowerSearch) ||
                       (t.LastName ?? string.Empty).ToLower().Contains(lowerSearch) ||
                       (t.Email ?? string.Empty).ToLower().Contains(lowerSearch) ||
                       (t.Department ?? string.Empty).ToLower().Contains(lowerSearch))
            .ToListAsync();
    }

    // UPDATED: Added filter parameters
    public async Task<int> GetTotalTeacherCountAsync(int? year = null, int? month = null)
    {
        var query = _context.Teachers.Where(t => !t.IsDeleted);

        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                // Filter by specific year and month using RegistrationDate
                query = query.Where(t => t.RegistrationDate.Year == year.Value &&
                                        t.RegistrationDate.Month == month.Value);
            }
            else
            {
                // Filter by year only using RegistrationDate
                query = query.Where(t => t.RegistrationDate.Year == year.Value);
            }
        }

        return await query.CountAsync();
    }

    public async Task<List<Teacher>> GetTeachersByDepartmentAsync(string department)
    {
        return await _context.Teachers
            .AsNoTracking()
            .Where(t => t.Department == department)
            .ToListAsync();
    }

    public async Task<List<GrowthData>> GetTeacherGrowthAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var query = _context.Teachers.Where(t => !t.IsDeleted);

        // Apply year/month filters using RegistrationDate
        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                query = query.Where(t => t.RegistrationDate.Year == year.Value &&
                                        t.RegistrationDate.Month == month.Value);
            }
            else
            {
                query = query.Where(t => t.RegistrationDate.Year == year.Value);
            }
        }

        var teachers = await query.ToListAsync();

        List<GrowthData> growthData = timeRange.ToLower() switch
        {
            "year" => GetYearlyGrowthData(teachers),
            "quarter" => GetQuarterlyGrowthData(teachers),
            _ => GetMonthlyGrowthData(teachers) // Default to monthly
        };

        return growthData;
    }

    public async Task<Dictionary<string, int>> GetDepartmentDistributionAsync()
    {
        var teachers = await _context.Teachers
            .Where(t => !t.IsDeleted)
            .ToListAsync();

        return teachers
            .GroupBy(t => t.Department ?? "Not Assigned")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetPositionDistributionAsync()
    {
        var teachers = await _context.Teachers
            .Where(t => !t.IsDeleted)
            .ToListAsync();

        return teachers
            .GroupBy(t => t.Position ?? "Not Specified")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // UPDATED: Added filter parameters and time range support
    public async Task<decimal> GetTeacherGrowthPercentageAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var currentDate = DateTime.Now;
        DateTime startDate, endDate;
        DateTime previousStartDate, previousEndDate;

        // Calculate date ranges based on timeRange
        switch (timeRange.ToLower())
        {
            case "year":
                startDate = new DateTime(currentDate.Year, 1, 1);
                endDate = new DateTime(currentDate.Year, 12, 31);
                previousStartDate = new DateTime(currentDate.Year - 1, 1, 1);
                previousEndDate = new DateTime(currentDate.Year - 1, 12, 31);
                break;
            case "quarter":
                var currentQuarter = (currentDate.Month - 1) / 3 + 1;
                startDate = new DateTime(currentDate.Year, (currentQuarter - 1) * 3 + 1, 1);
                endDate = startDate.AddMonths(3).AddDays(-1);
                previousStartDate = startDate.AddYears(-1);
                previousEndDate = endDate.AddYears(-1);
                break;
            default: // monthly
                startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = startDate.AddDays(-1);
                break;
        }

        // Apply year/month filters if provided
        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                startDate = new DateTime(year.Value, month.Value, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = startDate.AddDays(-1);
            }
            else
            {
                startDate = new DateTime(year.Value, 1, 1);
                endDate = new DateTime(year.Value, 12, 31);
                previousStartDate = startDate.AddYears(-1);
                previousEndDate = endDate.AddYears(-1);
            }
        }

        // UPDATED: Use RegistrationDate instead of CreatedAt
        var currentPeriodTeachers = await _context.Teachers
            .Where(t => !t.IsDeleted &&
                       t.RegistrationDate >= startDate &&
                       t.RegistrationDate <= endDate)
            .CountAsync();

        var previousPeriodTeachers = await _context.Teachers
            .Where(t => !t.IsDeleted &&
                       t.RegistrationDate >= previousStartDate &&
                       t.RegistrationDate <= previousEndDate)
            .CountAsync();

        if (previousPeriodTeachers == 0)
            return currentPeriodTeachers > 0 ? 100 : 0;

        return ((decimal)(currentPeriodTeachers - previousPeriodTeachers) / previousPeriodTeachers) * 100;
    }

    public async Task<int> GetActiveTeachersCountAsync()
    {
        return await _context.Teachers
            .Where(t => !t.IsDeleted)
            .CountAsync();
    }

    public async Task<int> GetNewTeachersThisMonthAsync()
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        return await _context.Teachers
            .Where(t => !t.IsDeleted &&
                       t.CreatedAt.Month == currentMonth &&
                       t.CreatedAt.Year == currentYear)
            .CountAsync();
    }

    public async Task<TeacherStatistics> GetTeacherStatisticsAsync()
    {
        var teachers = await _context.Teachers
            .Where(t => !t.IsDeleted)
            .ToListAsync();

        var subjectsPerTeacher = await _context.Teachers
            .Where(t => !t.IsDeleted)
            .Select(t => new { t.TeacherId, SubjectCount = t.Subjects.Count })
            .ToListAsync();

        return new TeacherStatistics
        {
            TotalTeachers = teachers.Count,
            AverageSubjectsPerTeacher = subjectsPerTeacher.Any() ? subjectsPerTeacher.Average(t => t.SubjectCount) : 0,
            DepartmentCount = teachers.Select(t => t.Department).Distinct().Count(),
            NewTeachersThisMonth = await GetNewTeachersThisMonthAsync()
        };
    }

    public async Task<List<TeacherWorkload>> GetTeacherWorkloadAsync()
    {
        var teachers = await _context.Teachers
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .Include(t => t.Subjects)
            .Select(t => new TeacherWorkload
            {
                TeacherId = t.TeacherId,
                FirstName = t.FirstName,
                LastName = t.LastName,
                Department = t.Department,
                SubjectCount = t.Subjects.Count,
                Email = t.Email
            })
            .OrderByDescending(t => t.SubjectCount)
            .ToListAsync();

        return teachers;
    }

    // NEW: Helper methods for different time ranges
    private List<GrowthData> GetMonthlyGrowthData(List<Teacher> teachers)
    {
        return teachers
            .GroupBy(t => new { t.RegistrationDate.Year, t.RegistrationDate.Month })
            .Select(g => new GrowthData
            {
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                CumulativeCount = teachers.Count(t =>
                    t.RegistrationDate.Year < g.Key.Year ||
                    (t.RegistrationDate.Year == g.Key.Year &&
                     t.RegistrationDate.Month <= g.Key.Month)),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private List<GrowthData> GetQuarterlyGrowthData(List<Teacher> teachers)
    {
        return teachers
            .GroupBy(t => new
            {
                Year = t.RegistrationDate.Year,
                Quarter = (t.RegistrationDate.Month - 1) / 3 + 1
            })
            .Select(g => new GrowthData
            {
                Label = $"Q{g.Key.Quarter} {g.Key.Year}",
                CumulativeCount = teachers.Count(t =>
                    t.RegistrationDate.Year < g.Key.Year ||
                    (t.RegistrationDate.Year == g.Key.Year &&
                     (t.RegistrationDate.Month - 1) / 3 + 1 <= g.Key.Quarter)),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private List<GrowthData> GetYearlyGrowthData(List<Teacher> teachers)
    {
        return teachers
            .GroupBy(t => t.RegistrationDate.Year)
            .Select(g => new GrowthData
            {
                Label = g.Key.ToString(),
                CumulativeCount = teachers.Count(t => t.RegistrationDate.Year <= g.Key),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    public async Task<int> GetNewTeachersCountAsync(int? year = null, int? month = null)
    {
        var query = _context.Teachers.Where(t => !t.IsDeleted);

        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                query = query.Where(t => t.RegistrationDate.Year == year.Value &&
                                        t.RegistrationDate.Month == month.Value);
            }
            else
            {
                query = query.Where(t => t.RegistrationDate.Year == year.Value);
            }
        }

        return await query.CountAsync();
    }
}

// Keep existing classes
public class TeacherGrowth
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int NewTeachers { get; set; }
    public int CumulativeTeachers { get; set; }
}

public class TeacherStatistics
{
    public int TotalTeachers { get; set; }
    public double AverageSubjectsPerTeacher { get; set; }
    public int DepartmentCount { get; set; }
    public int NewTeachersThisMonth { get; set; }
}

public class TeacherWorkload
{
    public int TeacherId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int SubjectCount { get; set; }
}