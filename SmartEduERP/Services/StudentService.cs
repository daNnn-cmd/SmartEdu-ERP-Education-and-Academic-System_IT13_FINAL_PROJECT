using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartEduERP.Services;

public class StudentService
{
    private readonly SmartEduDbContext _context;
    private readonly AuditLogService _auditLogService;
    private readonly ISyncQueueService _syncQueueService;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        SmartEduDbContext context,
        AuditLogService auditLogService,
        ISyncQueueService syncQueueService,
        ILogger<StudentService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _syncQueueService = syncQueueService;
        _logger = logger;
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        try
        {
            _logger.LogInformation("🔍 StudentService.GetAllStudentsAsync() called");

            // Get students with global query filter applied
            var filteredStudents = await _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            _logger.LogInformation($"✅ FILTERED STUDENTS COUNT (with global filter): {filteredStudents.Count} students");
            return filteredStudents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR in GetAllStudentsAsync");
            throw;
        }
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Subject)
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }

    public async Task<Student> CreateStudentAsync(Student student)
    {
        try
        {
            _logger.LogInformation("🔍 StudentService.CreateStudentAsync() called");
            _logger.LogInformation($"📝 Creating student: {student.FirstName} {student.LastName} ({student.Email})");

            // Run full DataAnnotations validation (enforces regex, max lengths, required fields, etc.)
            ValidateStudentModel(student);

            // Check if email already exists
            var existingStudent = await _context.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Email == student.Email);

            if (existingStudent != null)
                throw new InvalidOperationException($"A student with email '{student.Email}' already exists");

            // Set default values if not provided
            student.RegistrationDate = student.RegistrationDate ?? DateTime.Now;
            student.IsEmailConfirmed = student.IsEmailConfirmed;
            student.IsDeleted = false;
            student.EnrollmentStatus = string.IsNullOrWhiteSpace(student.EnrollmentStatus) ? "Active" : student.EnrollmentStatus;
            student.CreatedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;

            // Add student to context and save to get the StudentId
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Student created successfully with ID: {student.StudentId}");

            // ✅ AUTO-SYNC TO CLOUD
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000); // Small delay to ensure local save is complete

                    await _syncQueueService.QueueOperationAsync(
                        "Create",
                        "Students",
                        student.StudentId,
                        new
                        {
                            student.StudentId,
                            student.FirstName,
                            student.LastName,
                            student.Email,
                            student.EnrollmentStatus,
                            student.RegistrationDate
                        });

                    _logger.LogInformation($"✅ Student {student.StudentId} queued for cloud sync");
                }
                catch (Exception syncEx)
                {
                    _logger.LogError(syncEx, $"❌ Error queueing sync for student {student.StudentId}");
                }
            });

            // CREATE USER ACCOUNT FOR LOGIN
            await CreateUserAccountForStudent(student);

            // Log student creation
            try
            {
                var newStudentJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    student.StudentId,
                    student.FirstName,
                    student.LastName,
                    student.Email,
                    student.EnrollmentStatus,
                    student.RegistrationDate
                });

                await _auditLogService.LogActionAsync(
                    action: "Create",
                    tableName: "Students",
                    recordId: student.StudentId,
                    userId: null,
                    oldValues: null,
                    newValues: newStudentJson
                );
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "⚠️ Error logging audit");
            }

            return student;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR in CreateStudentAsync");
            throw;
        }
    }

    // Create user account for student
    private async Task CreateUserAccountForStudent(Student student)
    {
        try
        {
            _logger.LogInformation($"🎯 Creating user account for student: {student.FirstName} {student.LastName}");

            // Check if user account already exists for this email
            var existingUser = await _context.UserAccounts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == student.Email);

            if (existingUser != null)
            {
                _logger.LogWarning($"⚠️ User account already exists for email: {student.Email}");
                return;
            }

            // Generate username (firstname.lastname + random numbers)
            var baseUsername = $"{student.FirstName.ToLower()}.{student.LastName.ToLower()}";
            var username = baseUsername;
            var random = new Random();

            // Ensure username is unique
            while (await _context.UserAccounts.AnyAsync(u => u.Username == username))
            {
                username = $"{baseUsername}{random.Next(100, 999)}";
            }

            // Create user account
            var userAccount = new UserAccount
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Username = username,
                Email = student.Email,
                Password = HashPassword("Student123!"),
                Role = "Student",
                ReferenceId = student.StudentId,
                IsEmailConfirmed = student.IsEmailConfirmed,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserAccounts.Add(userAccount);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ User account created successfully: {username} (ID: {userAccount.UserId})");

            // Log user account creation
            try
            {
                var newUserJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    userAccount.UserId,
                    userAccount.Username,
                    userAccount.Role,
                    userAccount.ReferenceId
                });

                await _auditLogService.LogActionAsync(
                    action: "Create",
                    tableName: "UserAccounts",
                    recordId: userAccount.UserId,
                    userId: null,
                    oldValues: null,
                    newValues: newUserJson
                );
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "⚠️ Error logging user account audit");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR creating user account for student");
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public async Task<Student> CreateStudentFromUserAsync(int userId, string firstName, string lastName, string email,
        string? middleName = null, string? suffix = null, string? gender = null,
        string? contactNumber = null, string? address = null, DateTime? dateOfBirth = null)
    {
        var student = new Student
        {
            StudentId = userId,
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            Suffix = suffix,
            Email = email,
            ContactNumber = contactNumber ?? "",
            Address = address ?? "",
            DateOfBirth = dateOfBirth ?? DateTime.Now.AddYears(-18),
            Gender = gender,
            EnrollmentStatus = "Active",
            RegistrationDate = DateTime.Now,
            IsEmailConfirmed = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Sanitize and validate before saving/queuing for sync
        ValidateStudentModel(student);

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // ✅ AUTO-SYNC TO CLOUD
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000);

                await _syncQueueService.QueueOperationAsync(
                    "Create",
                    "Students",
                    student.StudentId,
                    new
                    {
                        student.StudentId,
                        student.FirstName,
                        student.LastName,
                        student.Email,
                        student.EnrollmentStatus
                    });

                _logger.LogInformation($"✅ Student {student.StudentId} (from user) queued for cloud sync");
            }
            catch (Exception syncEx)
            {
                _logger.LogError(syncEx, $"❌ Error queueing sync for student {student.StudentId}");
            }
        });

        // Log student creation
        try
        {
            var newStudentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                student.StudentId,
                student.FirstName,
                student.LastName,
                student.Email,
                student.EnrollmentStatus
            });

            await _auditLogService.LogActionAsync(
                action: "Create",
                tableName: "Students",
                recordId: student.StudentId,
                userId: null,
                oldValues: null,
                newValues: newStudentJson
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging audit");
        }

        return student;
    }

    private static void ValidateStudentModel(Student student)
    {
        // Centralized sanitization + DataAnnotations validation
        ValidationHelper.SanitizeAndValidateModel(student);
    }

    public async Task<Student?> UpdateStudentAsync(Student? student)
    {
        if (student == null)
            return null;

        var existing = await _context.Students.FindAsync(student.StudentId);
        if (existing == null)
            return null;

        // Run full DataAnnotations validation on the incoming model
        ValidateStudentModel(student);

        // Ensure email is unique among students (including soft-deleted ones)
        var otherStudentWithEmail = await _context.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Email == student.Email && s.StudentId != student.StudentId);

        if (otherStudentWithEmail != null)
            throw new InvalidOperationException($"A student with email '{student.Email}' already exists");

        // Capture old values before update
        var oldStudentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            existing.StudentId,
            existing.FirstName,
            existing.LastName,
            existing.Email,
            existing.EnrollmentStatus
        });

        existing.FirstName = student.FirstName;
        existing.LastName = student.LastName;
        existing.Email = student.Email;
        existing.ContactNumber = student.ContactNumber;
        existing.Address = student.Address;
        existing.DateOfBirth = student.DateOfBirth;
        existing.EnrollmentStatus = student.EnrollmentStatus;
        existing.Gender = student.Gender;
        existing.MiddleName = student.MiddleName;
        existing.Suffix = student.Suffix;
        existing.UpdatedAt = DateTime.UtcNow;

        // Keep any linked user account(s) in sync
        var linkedUsers = await _context.UserAccounts
            .IgnoreQueryFilters()
            .Where(u => u.ReferenceId == existing.StudentId &&
                        u.Role != null &&
                        u.Role.ToLower() == "student")
            .ToListAsync();

        foreach (var user in linkedUsers)
        {
            user.FirstName = existing.FirstName;
            user.LastName = existing.LastName;
            user.Email = existing.Email;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // ✅ AUTO-SYNC TO CLOUD
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000);

                await _syncQueueService.QueueOperationAsync(
                    "Update",
                    "Students",
                    existing.StudentId,
                    new
                    {
                        existing.StudentId,
                        existing.FirstName,
                        existing.LastName,
                        existing.Email,
                        existing.EnrollmentStatus
                    },
                    new
                    {
                        existing.StudentId,
                        existing.FirstName,
                        existing.LastName,
                        existing.Email,
                        existing.EnrollmentStatus
                    });

                _logger.LogInformation($"✅ Student {student.StudentId} update queued for cloud sync");
            }
            catch (Exception syncEx)
            {
                _logger.LogError(syncEx, $"❌ Error queueing update sync for student {student.StudentId}");
            }
        });

        // Log student update
        try
        {
            var newStudentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                existing.StudentId,
                existing.FirstName,
                existing.LastName,
                existing.Email,
                existing.EnrollmentStatus
            });

            await _auditLogService.LogActionAsync(
                action: "Update",
                tableName: "Students",
                recordId: student.StudentId,
                userId: null,
                oldValues: oldStudentJson,
                newValues: newStudentJson
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging audit");
        }

        return existing;
    }

    public async Task<bool> SoftDeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return false;

        // Capture student info before deletion
        var deletedStudentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            student.StudentId,
            student.FirstName,
            student.LastName,
            student.Email
        });

        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;

        // Also soft delete any linked user account(s) for this student
        var linkedUsers = await _context.UserAccounts
            .IgnoreQueryFilters()
            .Where(u => u.ReferenceId == student.StudentId &&
                        u.Role != null &&
                        u.Role.ToLower() == "student")
            .ToListAsync();

        foreach (var user in linkedUsers)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // ✅ AUTO-SYNC TO CLOUD
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000);

                await _syncQueueService.QueueOperationAsync(
                    "Delete",
                    "Students",
                    student.StudentId,
                    new
                    {
                        student.StudentId,
                        student.FirstName,
                        student.LastName,
                        student.Email
                    });

                _logger.LogInformation($"✅ Student {id} deletion queued for cloud sync");
            }
            catch (Exception syncEx)
            {
                _logger.LogError(syncEx, $"❌ Error queueing delete sync for student {id}");
            }
        });

        // Log student deletion
        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Students",
                recordId: id,
                userId: null,
                oldValues: deletedStudentJson,
                newValues: "Student soft deleted"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging audit");
        }

        return true;
    }

    public async Task<List<Student>> SearchStudentsAsync(string searchTerm)
    {
        var lowerSearch = searchTerm.ToLower();
        return await _context.Students
            .Where(s => s.FirstName!.ToLower().Contains(lowerSearch) ||
                       s.LastName!.ToLower().Contains(lowerSearch) ||
                       s.Email!.ToLower().Contains(lowerSearch))
            .ToListAsync();
    }

    public async Task<int> GetTotalStudentCountAsync(int? year = null, int? month = null)
    {
        var query = _context.Students.Where(s => !s.IsDeleted);

        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                query = query.Where(s => s.RegistrationDate.HasValue &&
                    s.RegistrationDate.Value.Year == year.Value &&
                    s.RegistrationDate.Value.Month == month.Value);
            }
            else
            {
                query = query.Where(s => s.RegistrationDate.HasValue &&
                    s.RegistrationDate.Value.Year == year.Value);
            }
        }

        return await query.CountAsync();
    }

    public async Task<List<GrowthData>> GetStudentGrowthAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var students = await _context.Students
            .Where(s => !s.IsDeleted)
            .ToListAsync();

        List<GrowthData> growthData = timeRange.ToLower() switch
        {
            "year" => GetYearlyGrowthData(students),
            "quarter" => GetQuarterlyGrowthData(students),
            _ => GetMonthlyGrowthData(students)
        };

        return growthData;
    }

    public async Task<decimal> GetStudentGrowthPercentageAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var currentDate = DateTime.Now;
        DateTime startDate, endDate;
        DateTime previousStartDate, previousEndDate;

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
            default:
                startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = startDate.AddDays(-1);
                break;
        }

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

        var currentPeriodStudents = await _context.Students
            .Where(s => !s.IsDeleted &&
                       s.RegistrationDate.HasValue &&
                       s.RegistrationDate.Value >= startDate &&
                       s.RegistrationDate.Value <= endDate)
            .CountAsync();

        var previousPeriodStudents = await _context.Students
            .Where(s => !s.IsDeleted &&
                       s.RegistrationDate.HasValue &&
                       s.RegistrationDate.Value >= previousStartDate &&
                       s.RegistrationDate.Value <= previousEndDate)
            .CountAsync();

        if (previousPeriodStudents == 0)
            return currentPeriodStudents > 0 ? 100 : 0;

        return ((decimal)(currentPeriodStudents - previousPeriodStudents) / previousPeriodStudents) * 100;
    }

    public async Task<Dictionary<string, int>> GetEnrollmentStatusDistributionAsync()
    {
        var students = await _context.Students
            .Where(s => !s.IsDeleted)
            .ToListAsync();

        return students
            .GroupBy(s => s.EnrollmentStatus ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetGenderDistributionAsync()
    {
        var students = await _context.Students
            .Where(s => !s.IsDeleted)
            .ToListAsync();

        return students
            .GroupBy(s => s.Gender ?? "Not Specified")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<AgeDistribution>> GetAgeDistributionAsync()
    {
        var students = await _context.Students
            .Where(s => !s.IsDeleted && s.DateOfBirth.HasValue)
            .ToListAsync();

        var ageGroups = students
            .Select(s => new
            {
                Age = DateTime.Now.Year - s.DateOfBirth!.Value.Year,
                Student = s
            })
            .GroupBy(s => GetAgeGroup(s.Age))
            .Select(g => new AgeDistribution
            {
                AgeGroup = g.Key,
                Count = g.Count(),
                Percentage = (double)g.Count() / students.Count * 100
            })
            .OrderBy(a => a.AgeGroup)
            .ToList();

        return ageGroups;
    }

    public async Task<int> GetActiveStudentsCountAsync()
    {
        return await _context.Students
            .Where(s => !s.IsDeleted && (s.EnrollmentStatus == "Active" || s.EnrollmentStatus == "Enrolled"))
            .CountAsync();
    }

    public async Task<int> GetNewStudentsThisMonthAsync()
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        return await _context.Students
            .Where(s => !s.IsDeleted &&
                       s.RegistrationDate.HasValue &&
                       s.RegistrationDate.Value.Month == currentMonth &&
                       s.RegistrationDate.Value.Year == currentYear)
            .CountAsync();
    }

    private List<GrowthData> GetMonthlyGrowthData(List<Student> students)
    {
        return students
            .GroupBy(s => new { s.RegistrationDate!.Value.Year, s.RegistrationDate!.Value.Month })
            .Select(g => new GrowthData
            {
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                CumulativeCount = students.Count(s =>
                    s.RegistrationDate!.Value.Year < g.Key.Year ||
                    (s.RegistrationDate!.Value.Year == g.Key.Year &&
                     s.RegistrationDate!.Value.Month <= g.Key.Month)),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private List<GrowthData> GetQuarterlyGrowthData(List<Student> students)
    {
        return students
            .GroupBy(s => new
            {
                Year = s.RegistrationDate!.Value.Year,
                Quarter = (s.RegistrationDate!.Value.Month - 1) / 3 + 1
            })
            .Select(g => new GrowthData
            {
                Label = $"Q{g.Key.Quarter} {g.Key.Year}",
                CumulativeCount = students.Count(s =>
                    s.RegistrationDate!.Value.Year < g.Key.Year ||
                    (s.RegistrationDate!.Value.Year == g.Key.Year &&
                     (s.RegistrationDate!.Value.Month - 1) / 3 + 1 <= g.Key.Quarter)),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private List<GrowthData> GetYearlyGrowthData(List<Student> students)
    {
        return students
            .GroupBy(s => s.RegistrationDate!.Value.Year)
            .Select(g => new GrowthData
            {
                Label = g.Key.ToString(),
                CumulativeCount = students.Count(s => s.RegistrationDate!.Value.Year <= g.Key),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private string GetAgeGroup(int age)
    {
        return age switch
        {
            < 18 => "Under 18",
            >= 18 and < 25 => "18-24",
            >= 25 and < 35 => "25-34",
            >= 35 and < 45 => "35-44",
            >= 45 and < 55 => "45-54",
            >= 55 and < 65 => "55-64",
            _ => "65+"
        };
    }
}

public class GrowthData
{
    public string Label { get; set; } = string.Empty;
    public int CumulativeCount { get; set; }
    public int NewCount { get; set; }
}

public class StudentGrowth
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int NewStudents { get; set; }
    public int CumulativeStudents { get; set; }
}

public class AgeDistribution
{
    public string AgeGroup { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}