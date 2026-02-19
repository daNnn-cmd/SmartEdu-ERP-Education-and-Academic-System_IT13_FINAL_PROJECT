using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class EnrollmentService
{
    private readonly SmartEduDbContext _context;
    private readonly ISyncQueueService _syncQueueService;
    private readonly AuditLogService _auditLogService;

    public EnrollmentService(SmartEduDbContext context, ISyncQueueService syncQueueService, AuditLogService auditLogService)
    {
        _context = context;
        _syncQueueService = syncQueueService;
        _auditLogService = auditLogService;
    }

    public async Task<List<Enrollment>> GetAllEnrollmentsAsync()
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .ThenInclude(s => s!.Teacher)
            .Include(e => e.ApprovedByUser)
            .Include(e => e.RejectedByUser)
            .OrderByDescending(e => e.EnrollmentDate) // Use EnrollmentDate
            .ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(int id)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .ThenInclude(s => s!.Teacher)
            .Include(e => e.ApprovedByUser)
            .Include(e => e.RejectedByUser)
            .Include(e => e.Grades)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }

    public async Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment)
    {
        // Sanitize and validate enrollment before saving or syncing
        ValidationHelper.SanitizeAndValidateModel(enrollment);

        enrollment.EnrollmentStatus = "Pending";
        enrollment.ApprovedByUserId = null;
        enrollment.ApprovedBy = null;
        enrollment.ApprovedAt = null;
        enrollment.RejectedByUserId = null;
        enrollment.RejectedBy = null;
        enrollment.RejectedAt = null;
        enrollment.RejectionReason = null;

        enrollment.CreatedAt = DateTime.UtcNow;
        enrollment.UpdatedAt = DateTime.UtcNow;

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        try
        {
            var newEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                enrollment.EnrollmentId,
                enrollment.StudentId,
                enrollment.SubjectId,
                enrollment.AcademicYear,
                enrollment.Semester,
                enrollment.EnrollmentDate,
                enrollment.EnrollmentStatus,
                enrollment.ApprovedByUserId,
                enrollment.ApprovedBy,
                enrollment.ApprovedAt,
                enrollment.RejectedByUserId,
                enrollment.RejectedBy,
                enrollment.RejectedAt,
                enrollment.RejectionReason
            });

            await _auditLogService.LogActionAsync(
                action: "Create",
                tableName: "Enrollments",
                recordId: enrollment.EnrollmentId,
                userId: null,
                oldValues: null,
                newValues: newEnrollmentJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging enrollment create audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Create",
                "Enrollments",
                enrollment.EnrollmentId,
                new
                {
                    enrollment.EnrollmentId,
                    enrollment.StudentId,
                    enrollment.SubjectId,
                    enrollment.AcademicYear,
                    enrollment.Semester,
                    enrollment.EnrollmentDate,
                    enrollment.EnrollmentStatus,
                    enrollment.ApprovedByUserId,
                    enrollment.ApprovedBy,
                    enrollment.ApprovedAt,
                    enrollment.RejectedByUserId,
                    enrollment.RejectedBy,
                    enrollment.RejectedAt,
                    enrollment.RejectionReason
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing enrollment create sync: {ex.Message}");
        }

        return enrollment;
    }

    public async Task<Enrollment?> UpdateEnrollmentAsync(Enrollment enrollment)
    {
        // Sanitize and validate enrollment before applying updates
        ValidationHelper.SanitizeAndValidateModel(enrollment);

        var existing = await _context.Enrollments.FindAsync(enrollment.EnrollmentId);
        if (existing == null)
            return null;

        if (!string.IsNullOrWhiteSpace(existing.EnrollmentStatus) &&
            !existing.EnrollmentStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            return null;

        var oldEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            existing.EnrollmentId,
            existing.StudentId,
            existing.SubjectId,
            existing.AcademicYear,
            existing.Semester,
            existing.EnrollmentDate,
            existing.EnrollmentStatus,
            existing.ApprovedByUserId,
            existing.ApprovedBy,
            existing.ApprovedAt,
            existing.RejectedByUserId,
            existing.RejectedBy,
            existing.RejectedAt,
            existing.RejectionReason
        });

        existing.StudentId = enrollment.StudentId;
        existing.SubjectId = enrollment.SubjectId;
        existing.AcademicYear = enrollment.AcademicYear;
        existing.Semester = enrollment.Semester;

        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            var newEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                existing.EnrollmentId,
                existing.StudentId,
                existing.SubjectId,
                existing.AcademicYear,
                existing.Semester,
                existing.EnrollmentDate,
                existing.EnrollmentStatus,
                existing.ApprovedByUserId,
                existing.ApprovedBy,
                existing.ApprovedAt,
                existing.RejectedByUserId,
                existing.RejectedBy,
                existing.RejectedAt,
                existing.RejectionReason
            });

            await _auditLogService.LogActionAsync(
                action: "Update",
                tableName: "Enrollments",
                recordId: existing.EnrollmentId,
                userId: null,
                oldValues: oldEnrollmentJson,
                newValues: newEnrollmentJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging enrollment update audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Enrollments",
                existing.EnrollmentId,
                new
                {
                    existing.EnrollmentId,
                    existing.StudentId,
                    existing.SubjectId,
                    existing.AcademicYear,
                    existing.Semester,
                    existing.EnrollmentDate,
                    existing.EnrollmentStatus,
                    existing.ApprovedByUserId,
                    existing.ApprovedBy,
                    existing.ApprovedAt,
                    existing.RejectedByUserId,
                    existing.RejectedBy,
                    existing.RejectedAt,
                    existing.RejectionReason
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing enrollment update sync: {ex.Message}");
        }

        return existing;
    }

    public async Task<bool> SoftDeleteEnrollmentAsync(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
            return false;

        var deletedEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            enrollment.EnrollmentId,
            enrollment.StudentId,
            enrollment.SubjectId,
            enrollment.AcademicYear,
            enrollment.Semester,
            enrollment.EnrollmentDate,
            enrollment.EnrollmentStatus,
            enrollment.ApprovedByUserId,
            enrollment.ApprovedBy,
            enrollment.ApprovedAt,
            enrollment.RejectedByUserId,
            enrollment.RejectedBy,
            enrollment.RejectedAt,
            enrollment.RejectionReason
        });

        enrollment.IsDeleted = true;
        enrollment.DeletedAt = DateTime.UtcNow;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Enrollments",
                recordId: enrollment.EnrollmentId,
                userId: null,
                oldValues: deletedEnrollmentJson,
                newValues: "Enrollment soft deleted"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging enrollment delete audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Delete",
                "Enrollments",
                enrollment.EnrollmentId,
                new
                {
                    enrollment.EnrollmentId,
                    enrollment.StudentId,
                    enrollment.SubjectId,
                    enrollment.AcademicYear,
                    enrollment.Semester,
                    enrollment.EnrollmentDate,
                    enrollment.EnrollmentStatus,
                    enrollment.ApprovedByUserId,
                    enrollment.ApprovedBy,
                    enrollment.ApprovedAt,
                    enrollment.RejectedByUserId,
                    enrollment.RejectedBy,
                    enrollment.RejectedAt,
                    enrollment.RejectionReason
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing enrollment delete sync: {ex.Message}");
        }

        return true;
    }

    public async Task<List<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
    {
        return await _context.Enrollments
            .Include(e => e.Subject)
            .ThenInclude(s => s!.Teacher)
            .Include(e => e.ApprovedByUser)
            .Include(e => e.RejectedByUser)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<List<Enrollment>> GetEnrollmentsBySubjectAsync(int subjectId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.ApprovedByUser)
            .Include(e => e.RejectedByUser)
            .Where(e => e.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<bool> ApproveEnrollmentAsync(int enrollmentId, int approvedByUserId, string? approvedBy)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment == null || enrollment.IsDeleted)
            return false;

        if (!string.Equals(enrollment.EnrollmentStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            return false;

        var oldEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            enrollment.EnrollmentId,
            enrollment.StudentId,
            enrollment.SubjectId,
            enrollment.AcademicYear,
            enrollment.Semester,
            enrollment.EnrollmentDate,
            enrollment.EnrollmentStatus,
            enrollment.ApprovedByUserId,
            enrollment.ApprovedBy,
            enrollment.ApprovedAt,
            enrollment.RejectedByUserId,
            enrollment.RejectedBy,
            enrollment.RejectedAt,
            enrollment.RejectionReason
        });

        enrollment.EnrollmentStatus = "Approved";
        enrollment.ApprovedByUserId = approvedByUserId;
        enrollment.ApprovedBy = approvedBy;
        enrollment.ApprovedAt = DateTime.UtcNow;
        enrollment.RejectedByUserId = null;
        enrollment.RejectedBy = null;
        enrollment.RejectedAt = null;
        enrollment.RejectionReason = null;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            var newEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                enrollment.EnrollmentId,
                enrollment.StudentId,
                enrollment.SubjectId,
                enrollment.AcademicYear,
                enrollment.Semester,
                enrollment.EnrollmentDate,
                enrollment.EnrollmentStatus,
                enrollment.ApprovedByUserId,
                enrollment.ApprovedBy,
                enrollment.ApprovedAt,
                enrollment.RejectedByUserId,
                enrollment.RejectedBy,
                enrollment.RejectedAt,
                enrollment.RejectionReason
            });

            await _auditLogService.LogActionAsync(
                action: "ApproveEnrollment",
                tableName: "Enrollments",
                recordId: enrollment.EnrollmentId,
                userId: approvedByUserId,
                oldValues: oldEnrollmentJson,
                newValues: newEnrollmentJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging enrollment approve audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Enrollments",
                enrollment.EnrollmentId,
                new
                {
                    enrollment.EnrollmentId,
                    enrollment.StudentId,
                    enrollment.SubjectId,
                    enrollment.AcademicYear,
                    enrollment.Semester,
                    enrollment.EnrollmentDate,
                    enrollment.EnrollmentStatus,
                    enrollment.ApprovedByUserId,
                    enrollment.ApprovedBy,
                    enrollment.ApprovedAt,
                    enrollment.RejectedByUserId,
                    enrollment.RejectedBy,
                    enrollment.RejectedAt,
                    enrollment.RejectionReason
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing enrollment approve sync: {ex.Message}");
        }

        return true;
    }

    public async Task<bool> RejectEnrollmentAsync(int enrollmentId, int rejectedByUserId, string? rejectedBy, string? rejectionReason)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment == null || enrollment.IsDeleted)
            return false;

        if (!string.Equals(enrollment.EnrollmentStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            return false;

        var oldEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            enrollment.EnrollmentId,
            enrollment.StudentId,
            enrollment.SubjectId,
            enrollment.AcademicYear,
            enrollment.Semester,
            enrollment.EnrollmentDate,
            enrollment.EnrollmentStatus,
            enrollment.ApprovedByUserId,
            enrollment.ApprovedBy,
            enrollment.ApprovedAt,
            enrollment.RejectedByUserId,
            enrollment.RejectedBy,
            enrollment.RejectedAt,
            enrollment.RejectionReason
        });

        enrollment.EnrollmentStatus = "Rejected";
        enrollment.RejectedByUserId = rejectedByUserId;
        enrollment.RejectedBy = rejectedBy;
        enrollment.RejectedAt = DateTime.UtcNow;
        enrollment.RejectionReason = rejectionReason;
        enrollment.ApprovedByUserId = null;
        enrollment.ApprovedBy = null;
        enrollment.ApprovedAt = null;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            var newEnrollmentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                enrollment.EnrollmentId,
                enrollment.StudentId,
                enrollment.SubjectId,
                enrollment.AcademicYear,
                enrollment.Semester,
                enrollment.EnrollmentDate,
                enrollment.EnrollmentStatus,
                enrollment.ApprovedByUserId,
                enrollment.ApprovedBy,
                enrollment.ApprovedAt,
                enrollment.RejectedByUserId,
                enrollment.RejectedBy,
                enrollment.RejectedAt,
                enrollment.RejectionReason
            });

            await _auditLogService.LogActionAsync(
                action: "RejectEnrollment",
                tableName: "Enrollments",
                recordId: enrollment.EnrollmentId,
                userId: rejectedByUserId,
                oldValues: oldEnrollmentJson,
                newValues: newEnrollmentJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging enrollment reject audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Enrollments",
                enrollment.EnrollmentId,
                new
                {
                    enrollment.EnrollmentId,
                    enrollment.StudentId,
                    enrollment.SubjectId,
                    enrollment.AcademicYear,
                    enrollment.Semester,
                    enrollment.EnrollmentDate,
                    enrollment.EnrollmentStatus,
                    enrollment.ApprovedByUserId,
                    enrollment.ApprovedBy,
                    enrollment.ApprovedAt,
                    enrollment.RejectedByUserId,
                    enrollment.RejectedBy,
                    enrollment.RejectedAt,
                    enrollment.RejectionReason
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing enrollment reject sync: {ex.Message}");
        }

        return true;
    }

    public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        return await GetEnrollmentsByStudentAsync(studentId);
    }

    public async Task<List<Student>> GetCurrentStudentsForTeacherAsync(int teacherId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Where(e => e.Subject != null
                        && e.Subject.TeacherId == teacherId
                        && e.Student != null
                        && (e.Student.EnrollmentStatus == "Active" || e.Student.EnrollmentStatus == "Enrolled"))
            .ToListAsync();

        return enrollments
            .Where(e => e.Student != null)
            .Select(e => e.Student!)
            .GroupBy(s => s.StudentId)
            .Select(g => g.First())
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToList();
    }

    // UPDATED: Use EnrollmentDate for filtering
    public async Task<int> GetTotalEnrollmentCountAsync(int? year = null, int? month = null)
    {
        var query = _context.Enrollments
            .Where(e => !e.IsDeleted); // Only count non-deleted enrollments

        // Apply year/month filters using EnrollmentDate
        if (year.HasValue && month.HasValue)
        {
            // Specific year and month: June 2023
            query = query.Where(e =>
                e.EnrollmentDate.Year == year.Value &&
                e.EnrollmentDate.Month == month.Value
            );
        }
        else if (year.HasValue)
        {
            // Only year filter: All of 2024
            query = query.Where(e => e.EnrollmentDate.Year == year.Value);
        }
        else if (month.HasValue)
        {
            // Only month filter: June of any year
            query = query.Where(e => e.EnrollmentDate.Month == month.Value);
        }

        return await query.CountAsync();
    }

    // UPDATED: Use EnrollmentDate for filtering
    public async Task<List<EnrollmentGrowthData>> GetEnrollmentGrowthAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var query = _context.Enrollments.Where(e => !e.IsDeleted);

        // Apply year/month filters using EnrollmentDate
        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                query = query.Where(e => e.EnrollmentDate.Year == year.Value &&
                                        e.EnrollmentDate.Month == month.Value);
            }
            else
            {
                query = query.Where(e => e.EnrollmentDate.Year == year.Value);
            }
        }

        var enrollments = await query.ToListAsync();

        List<EnrollmentGrowthData> growthData = timeRange.ToLower() switch
        {
            "year" => GetYearlyGrowthData(enrollments),
            "quarter" => GetQuarterlyGrowthData(enrollments),
            _ => GetMonthlyGrowthData(enrollments) // Default to monthly
        };

        return growthData;
    }

    public async Task<Dictionary<string, int>> GetEnrollmentByAcademicYearAsync()
    {
        var enrollments = await _context.Enrollments
            .Where(e => !e.IsDeleted)
            .ToListAsync();

        return enrollments
            .GroupBy(e => e.AcademicYear ?? "Not Specified")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetEnrollmentBySemesterAsync()
    {
        var enrollments = await _context.Enrollments
            .Where(e => !e.IsDeleted)
            .ToListAsync();

        return enrollments
            .GroupBy(e => e.Semester ?? "Not Specified")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetEnrollmentBySubjectAsync()
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Subject)
            .Where(e => !e.IsDeleted && e.Subject != null)
            .ToListAsync();

        return enrollments
            .GroupBy(e => e.Subject!.SubjectName ?? "Unknown Subject")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // UPDATED: Use EnrollmentDate for growth percentage calculations
    public async Task<decimal> GetEnrollmentGrowthPercentageAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var currentCount = await GetTotalEnrollmentCountAsync(year, month);

        // Calculate previous period based on filters
        int? previousYear = year;
        int? previousMonth = month;

        if (month.HasValue && month > 1)
        {
            previousMonth = month.Value - 1;
            previousYear = year;
        }
        else if (month.HasValue && month == 1)
        {
            previousMonth = 12;
            previousYear = year.HasValue ? year.Value - 1 : null;
        }
        else if (year.HasValue)
        {
            previousYear = year.Value - 1;
            previousMonth = month;
        }
        else
        {
            // If no filters, compare with previous month
            var lastMonth = DateTime.Now.AddMonths(-1);
            previousYear = lastMonth.Year;
            previousMonth = lastMonth.Month;
        }

        var previousCount = await GetTotalEnrollmentCountAsync(previousYear, previousMonth);

        if (previousCount == 0)
            return currentCount > 0 ? 100 : 0;

        return ((currentCount - previousCount) / (decimal)previousCount) * 100;
    }

    public async Task<int> GetActiveEnrollmentsCountAsync()
    {
        return await _context.Enrollments
            .Where(e => !e.IsDeleted)
            .CountAsync();
    }

    // UPDATED: Use EnrollmentDate for this month count
    public async Task<int> GetNewEnrollmentsThisMonthAsync()
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        return await _context.Enrollments
            .Where(e => !e.IsDeleted &&
                       e.EnrollmentDate.Month == currentMonth &&
                       e.EnrollmentDate.Year == currentYear)
            .CountAsync();
    }

    public async Task<EnrollmentStatistics> GetEnrollmentStatisticsAsync()
    {
        var enrollments = await _context.Enrollments
            .Where(e => !e.IsDeleted)
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .ToListAsync();

        var studentsWithEnrollments = enrollments
            .Select(e => e.StudentId)
            .Distinct()
            .Count();

        var subjectsWithEnrollments = enrollments
            .Select(e => e.SubjectId)
            .Distinct()
            .Count();

        var averageEnrollmentsPerStudent = studentsWithEnrollments > 0
            ? (double)enrollments.Count / studentsWithEnrollments
            : 0;

        return new EnrollmentStatistics
        {
            TotalEnrollments = enrollments.Count,
            StudentsWithEnrollments = studentsWithEnrollments,
            SubjectsWithEnrollments = subjectsWithEnrollments,
            AverageEnrollmentsPerStudent = averageEnrollmentsPerStudent,
            NewEnrollmentsThisMonth = await GetNewEnrollmentsThisMonthAsync()
        };
    }

    public async Task<List<SubjectEnrollment>> GetTopEnrolledSubjectsAsync(int topCount = 10)
    {
        var subjectEnrollments = await _context.Enrollments
            .Include(e => e.Subject)
            .Where(e => !e.IsDeleted && e.Subject != null)
            .GroupBy(e => new { e.SubjectId, e.Subject!.SubjectName })
            .Select(g => new SubjectEnrollment
            {
                SubjectId = g.Key.SubjectId,
                SubjectName = g.Key.SubjectName,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(se => se.EnrollmentCount)
            .Take(topCount)
            .ToListAsync();

        return subjectEnrollments;
    }

    public async Task<List<StudentEnrollment>> GetStudentsWithMostEnrollmentsAsync(int topCount = 10)
    {
        var studentEnrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => !e.IsDeleted && e.Student != null)
            .GroupBy(e => new { e.StudentId, e.Student!.FirstName, e.Student.LastName })
            .Select(g => new StudentEnrollment
            {
                StudentId = g.Key.StudentId,
                FirstName = g.Key.FirstName,
                LastName = g.Key.LastName,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(se => se.EnrollmentCount)
            .Take(topCount)
            .ToListAsync();

        return studentEnrollments;
    }

    // UPDATED: Helper methods using EnrollmentDate
    private List<EnrollmentGrowthData> GetMonthlyGrowthData(List<Enrollment> enrollments)
    {
        return enrollments
            .GroupBy(e => new { e.EnrollmentDate.Year, e.EnrollmentDate.Month })
            .Select(g => new EnrollmentGrowthData
            {
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                CumulativeCount = enrollments.Count(e =>
                    e.EnrollmentDate.Year < g.Key.Year ||
                    (e.EnrollmentDate.Year == g.Key.Year &&
                     e.EnrollmentDate.Month <= g.Key.Month)),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private List<EnrollmentGrowthData> GetQuarterlyGrowthData(List<Enrollment> enrollments)
    {
        return enrollments
            .GroupBy(e => new
            {
                Year = e.EnrollmentDate.Year,
                Quarter = (e.EnrollmentDate.Month - 1) / 3 + 1
            })
            .Select(g => new EnrollmentGrowthData
            {
                Label = $"Q{g.Key.Quarter} {g.Key.Year}",
                CumulativeCount = enrollments.Count(e =>
                    e.EnrollmentDate.Year < g.Key.Year ||
                    (e.EnrollmentDate.Year == g.Key.Year &&
                     (e.EnrollmentDate.Month - 1) / 3 + 1 <= g.Key.Quarter)),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    private List<EnrollmentGrowthData> GetYearlyGrowthData(List<Enrollment> enrollments)
    {
        return enrollments
            .GroupBy(e => e.EnrollmentDate.Year)
            .Select(g => new EnrollmentGrowthData
            {
                Label = g.Key.ToString(),
                CumulativeCount = enrollments.Count(e => e.EnrollmentDate.Year <= g.Key),
                NewCount = g.Count()
            })
            .OrderBy(g => g.Label)
            .ToList();
    }

    // UPDATED: Use EnrollmentDate for growth data
    public async Task<List<EnrollmentGrowthData>> GetEnrollmentGrowthDataAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var query = _context.Enrollments.Where(e => !e.IsDeleted);

        // Apply year/month filters using EnrollmentDate
        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                query = query.Where(e => e.EnrollmentDate.Year == year.Value &&
                                        e.EnrollmentDate.Month == month.Value);
            }
            else
            {
                query = query.Where(e => e.EnrollmentDate.Year == year.Value);
            }
        }

        var enrollments = await query.ToListAsync();

        return timeRange.ToLower() switch
        {
            "year" => GetYearlyGrowthData(enrollments),
            "quarter" => GetQuarterlyGrowthData(enrollments),
            _ => GetMonthlyGrowthData(enrollments)
        };
    }

    public async Task<Dictionary<string, int>> GetEnrollmentDistributionAsync()
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Subject)
            .Where(e => !e.IsDeleted)
            .ToListAsync();

        // Group by academic year for distribution
        return enrollments
            .GroupBy(e => e.AcademicYear ?? "Not Specified")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Helper method to generate distinct colors
    private string[] GenerateColors(int count)
    {
        var colors = new[]
        {
            "rgba(59, 130, 246, 0.8)",   // Blue
            "rgba(16, 185, 129, 0.8)",   // Green
            "rgba(245, 158, 11, 0.8)",   // Yellow
            "rgba(139, 92, 246, 0.8)",   // Purple
            "rgba(236, 72, 153, 0.8)",   // Pink
            "rgba(14, 165, 233, 0.8)",   // Light Blue
            "rgba(34, 197, 94, 0.8)",    // Light Green
            "rgba(249, 115, 22, 0.8)",   // Orange
            "rgba(168, 85, 247, 0.8)",   // Light Purple
            "rgba(239, 68, 68, 0.8)"     // Red
        };

        return colors.Take(count).ToArray();
    }
}

// Keep existing classes
public class EnrollmentGrowthData
{
    public string Label { get; set; } = string.Empty;
    public int CumulativeCount { get; set; }
    public int NewCount { get; set; }
}

public class EnrollmentTrend
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int NewEnrollments { get; set; }
    public int CumulativeEnrollments { get; set; }
}

public class EnrollmentStatistics
{
    public int TotalEnrollments { get; set; }
    public int StudentsWithEnrollments { get; set; }
    public int SubjectsWithEnrollments { get; set; }
    public double AverageEnrollmentsPerStudent { get; set; }
    public int NewEnrollmentsThisMonth { get; set; }
}

public class SubjectEnrollment
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}

public class StudentEnrollment
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}