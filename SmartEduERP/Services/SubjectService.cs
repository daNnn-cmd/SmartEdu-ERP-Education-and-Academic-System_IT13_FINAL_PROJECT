using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class SubjectService
{
    private readonly SmartEduDbContext _context;
    private readonly ISyncQueueService _syncQueueService;
    private readonly AuditLogService _auditLogService;

    public SubjectService(SmartEduDbContext context, ISyncQueueService syncQueueService, AuditLogService auditLogService)
    {
        _context = context;
        _syncQueueService = syncQueueService;
        _auditLogService = auditLogService;
    }

    public async Task<List<Subject>> GetAllSubjectsAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.ProposedByUser)
            .Include(s => s.NotedByUser)
            .Include(s => s.ApprovedByUser)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.GradeLevel)
            .ThenBy(s => s.Section)
            .ThenBy(s => s.SubjectCode)
            .ToListAsync();
    }

    public async Task<List<Subject>> GetAllSubjectsIncludingArchivedAsync()
    {
        return await _context.Subjects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.ProposedByUser)
            .Include(s => s.NotedByUser)
            .Include(s => s.ApprovedByUser)
            .OrderBy(s => s.IsDeleted)
            .ThenBy(s => s.GradeLevel)
            .ThenBy(s => s.Section)
            .ThenBy(s => s.SubjectCode)
            .ToListAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(int id)
    {
        return await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.SubjectId == id && !s.IsDeleted);
    }

    public async Task<Subject> CreateSubjectAsync(Subject subject, int? userId = null)
    {
        // Generate subject code if not provided
        if (string.IsNullOrEmpty(subject.SubjectCode))
        {
            subject.SubjectCode = await GenerateSubjectCodeAsync(subject);
        }

        // Set timestamps
        subject.CreatedAt = DateTime.UtcNow;
        subject.UpdatedAt = DateTime.UtcNow;
        subject.IsDeleted = false;

        // Ensure proposal/approval workflow defaults
        if (string.IsNullOrWhiteSpace(subject.SubjectStatus))
        {
            subject.SubjectStatus = "Proposed";
        }

        if (!subject.ProposedByUserId.HasValue && userId.HasValue)
        {
            subject.ProposedByUserId = userId.Value;
            subject.ProposedAt = DateTime.UtcNow;
        }

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        try
        {
            var newSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                subject.SubjectId,
                subject.SubjectName,
                subject.SubjectCode,
                subject.GradeLevel,
                subject.Section,
                subject.AcademicYear,
                subject.TeacherId,
                subject.SubjectStatus,
                subject.ProposedByUserId,
                subject.ProposedBy,
                subject.ProposedAt,
                subject.NotedByUserId,
                subject.NotedBy,
                subject.NotedAt,
                subject.ApprovedByUserId,
                subject.ApprovedBy,
                subject.ApprovedAt
            });

            await _auditLogService.LogActionAsync(
                action: "Create",
                tableName: "Subjects",
                recordId: subject.SubjectId,
                userId: userId,
                oldValues: null,
                newValues: newSubjectJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging subject create audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Create",
                "Subjects",
                subject.SubjectId,
                new
                {
                    subject.SubjectId,
                    subject.SubjectName,
                    subject.SubjectCode,
                    subject.GradeLevel,
                    subject.Section,
                    subject.AcademicYear,
                    subject.TeacherId,
                    subject.SubjectStatus,
                    subject.ProposedByUserId,
                    subject.ProposedBy,
                    subject.ProposedAt,
                    subject.NotedByUserId,
                    subject.NotedBy,
                    subject.NotedAt,
                    subject.ApprovedByUserId,
                    subject.ApprovedBy,
                    subject.ApprovedAt
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing subject create sync: {ex.Message}");
        }

        return subject;
    }

    private async Task<string> GenerateSubjectCodeAsync(Subject subject)
    {
        // Get first 3 letters of subject name
        var subjectPrefix = subject.SubjectName.Length >= 3
            ? subject.SubjectName.Substring(0, 3).ToUpper()
            : subject.SubjectName.ToUpper().PadRight(3, 'X');

        // Extract grade number
        var gradeNumber = System.Text.RegularExpressions.Regex.Match(subject.GradeLevel, @"\d+").Value;
        if (string.IsNullOrEmpty(gradeNumber))
            gradeNumber = "00";

        var baseCode = $"{subjectPrefix}-{gradeNumber}{subject.Section}";
        var finalCode = baseCode;
        var counter = 1;

        // Check for duplicates
        while (await _context.Subjects.AnyAsync(s => s.SubjectCode == finalCode))
        {
            finalCode = $"{baseCode}-{counter}";
            counter++;
            if (counter > 99) break; // Safety limit
        }

        return finalCode;
    }

    public async Task<Subject?> UpdateSubjectAsync(Subject subject)
    {
        var existing = await _context.Subjects.FindAsync(subject.SubjectId);
        if (existing == null || existing.IsDeleted)
            return null;

        var oldSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            existing.SubjectId,
            existing.SubjectName,
            existing.SubjectCode,
            existing.GradeLevel,
            existing.Section,
            existing.AcademicYear,
            existing.TeacherId,
            existing.SubjectStatus,
            existing.ProposedByUserId,
            existing.ProposedBy,
            existing.ProposedAt,
            existing.NotedByUserId,
            existing.NotedBy,
            existing.NotedAt,
            existing.ApprovedByUserId,
            existing.ApprovedBy,
            existing.ApprovedAt
        });

        existing.SubjectName = subject.SubjectName;
        existing.GradeLevel = subject.GradeLevel;
        existing.Section = subject.Section;
        existing.AcademicYear = subject.AcademicYear;
        existing.TeacherId = subject.TeacherId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            var newSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                existing.SubjectId,
                existing.SubjectName,
                existing.SubjectCode,
                existing.GradeLevel,
                existing.Section,
                existing.AcademicYear,
                existing.TeacherId,
                existing.SubjectStatus,
                existing.ProposedByUserId,
                existing.ProposedBy,
                existing.ProposedAt,
                existing.NotedByUserId,
                existing.NotedBy,
                existing.NotedAt,
                existing.ApprovedByUserId,
                existing.ApprovedBy,
                existing.ApprovedAt
            });

            await _auditLogService.LogActionAsync(
                action: "Update",
                tableName: "Subjects",
                recordId: existing.SubjectId,
                userId: null,
                oldValues: oldSubjectJson,
                newValues: newSubjectJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging subject update audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Subjects",
                existing.SubjectId,
                new
                {
                    existing.SubjectId,
                    existing.SubjectName,
                    existing.SubjectCode,
                    existing.GradeLevel,
                    existing.Section,
                    existing.AcademicYear,
                    existing.TeacherId,
                    existing.SubjectStatus,
                    existing.ProposedByUserId,
                    existing.ProposedBy,
                    existing.ProposedAt,
                    existing.NotedByUserId,
                    existing.NotedBy,
                    existing.NotedAt,
                    existing.ApprovedByUserId,
                    existing.ApprovedBy,
                    existing.ApprovedAt
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing subject update sync: {ex.Message}");
        }

        return existing;
    }

    public async Task<bool> SoftDeleteSubjectAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null || subject.IsDeleted)
            return false;

        var deletedSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            subject.SubjectId,
            subject.SubjectName,
            subject.SubjectCode,
            subject.GradeLevel,
            subject.Section,
            subject.AcademicYear,
            subject.TeacherId,
            subject.SubjectStatus,
            subject.ProposedByUserId,
            subject.ProposedBy,
            subject.ProposedAt,
            subject.NotedByUserId,
            subject.NotedBy,
            subject.NotedAt,
            subject.ApprovedByUserId,
            subject.ApprovedBy,
            subject.ApprovedAt
        });

        subject.IsDeleted = true;
        subject.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Subjects",
                recordId: subject.SubjectId,
                userId: null,
                oldValues: deletedSubjectJson,
                newValues: "Subject soft deleted"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging subject delete audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Delete",
                "Subjects",
                subject.SubjectId,
                new
                {
                    subject.SubjectId,
                    subject.SubjectName,
                    subject.SubjectCode,
                    subject.GradeLevel,
                    subject.Section,
                    subject.AcademicYear,
                    subject.TeacherId,
                    subject.SubjectStatus,
                    subject.ProposedByUserId,
                    subject.ProposedBy,
                    subject.ProposedAt,
                    subject.NotedByUserId,
                    subject.NotedBy,
                    subject.NotedAt,
                    subject.ApprovedByUserId,
                    subject.ApprovedBy,
                    subject.ApprovedAt
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing subject delete sync: {ex.Message}");
        }

        return true;
    }

    public async Task<bool> NoteSubjectAsync(int subjectId, int notedByUserId, string? notedBy)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null || subject.IsDeleted)
            return false;

        var oldSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            subject.SubjectId,
            subject.SubjectName,
            subject.SubjectCode,
            subject.GradeLevel,
            subject.Section,
            subject.AcademicYear,
            subject.TeacherId,
            subject.SubjectStatus,
            subject.ProposedByUserId,
            subject.ProposedBy,
            subject.ProposedAt,
            subject.NotedByUserId,
            subject.NotedBy,
            subject.NotedAt,
            subject.ApprovedByUserId,
            subject.ApprovedBy,
            subject.ApprovedAt
        });

        subject.SubjectStatus = "Noted";
        subject.NotedByUserId = notedByUserId;
        subject.NotedBy = notedBy;
        subject.NotedAt = DateTime.UtcNow;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            var newSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                subject.SubjectId,
                subject.SubjectName,
                subject.SubjectCode,
                subject.GradeLevel,
                subject.Section,
                subject.AcademicYear,
                subject.TeacherId,
                subject.SubjectStatus,
                subject.ProposedByUserId,
                subject.ProposedBy,
                subject.ProposedAt,
                subject.NotedByUserId,
                subject.NotedBy,
                subject.NotedAt,
                subject.ApprovedByUserId,
                subject.ApprovedBy,
                subject.ApprovedAt
            });

            await _auditLogService.LogActionAsync(
                action: "NoteSubject",
                tableName: "Subjects",
                recordId: subject.SubjectId,
                userId: notedByUserId,
                oldValues: oldSubjectJson,
                newValues: newSubjectJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging subject note audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Subjects",
                subject.SubjectId,
                new
                {
                    subject.SubjectId,
                    subject.SubjectName,
                    subject.SubjectCode,
                    subject.GradeLevel,
                    subject.Section,
                    subject.AcademicYear,
                    subject.TeacherId,
                    subject.SubjectStatus,
                    subject.ProposedByUserId,
                    subject.ProposedBy,
                    subject.ProposedAt,
                    subject.NotedByUserId,
                    subject.NotedBy,
                    subject.NotedAt,
                    subject.ApprovedByUserId,
                    subject.ApprovedBy,
                    subject.ApprovedAt
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing subject note sync: {ex.Message}");
        }

        return true;
    }

    public async Task<bool> ApproveSubjectAsync(int subjectId, int approvedByUserId, string? approvedBy)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null || subject.IsDeleted)
            return false;

        var oldSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            subject.SubjectId,
            subject.SubjectName,
            subject.SubjectCode,
            subject.GradeLevel,
            subject.Section,
            subject.AcademicYear,
            subject.TeacherId,
            subject.SubjectStatus,
            subject.ProposedByUserId,
            subject.ProposedBy,
            subject.ProposedAt,
            subject.NotedByUserId,
            subject.NotedBy,
            subject.NotedAt,
            subject.ApprovedByUserId,
            subject.ApprovedBy,
            subject.ApprovedAt
        });

        subject.SubjectStatus = "Approved";
        subject.ApprovedByUserId = approvedByUserId;
        subject.ApprovedBy = approvedBy;
        subject.ApprovedAt = DateTime.UtcNow;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            var newSubjectJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                subject.SubjectId,
                subject.SubjectName,
                subject.SubjectCode,
                subject.GradeLevel,
                subject.Section,
                subject.AcademicYear,
                subject.TeacherId,
                subject.SubjectStatus,
                subject.ProposedByUserId,
                subject.ProposedBy,
                subject.ProposedAt,
                subject.NotedByUserId,
                subject.NotedBy,
                subject.NotedAt,
                subject.ApprovedByUserId,
                subject.ApprovedBy,
                subject.ApprovedAt
            });

            await _auditLogService.LogActionAsync(
                action: "ApproveSubject",
                tableName: "Subjects",
                recordId: subject.SubjectId,
                userId: approvedByUserId,
                oldValues: oldSubjectJson,
                newValues: newSubjectJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging subject approve audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Subjects",
                subject.SubjectId,
                new
                {
                    subject.SubjectId,
                    subject.SubjectName,
                    subject.SubjectCode,
                    subject.GradeLevel,
                    subject.Section,
                    subject.AcademicYear,
                    subject.TeacherId,
                    subject.SubjectStatus,
                    subject.ProposedByUserId,
                    subject.ProposedBy,
                    subject.ProposedAt,
                    subject.NotedByUserId,
                    subject.NotedBy,
                    subject.NotedAt,
                    subject.ApprovedByUserId,
                    subject.ApprovedBy,
                    subject.ApprovedAt
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing subject approve sync: {ex.Message}");
        }

        return true;
    }

    // ADD THIS METHOD - For Teacher Dashboard
    public async Task<List<Subject>> GetSubjectsByTeacherIdAsync(int teacherId)
    {
        return await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Where(s => s.TeacherId == teacherId && !s.IsDeleted)
            .OrderBy(s => s.GradeLevel)
            .ThenBy(s => s.Section)
            .ThenBy(s => s.SubjectName)
            .ToListAsync();
    }

    // Additional helper methods
    public async Task<List<Subject>> GetSubjectsByGradeLevelAsync(string gradeLevel)
    {
        return await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Where(s => s.GradeLevel == gradeLevel && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<string>> GetAvailableGradeLevelsAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Select(s => s.GradeLevel)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
    }

    public async Task<List<string>> GetAvailableSectionsAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Select(s => s.Section)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    // Search method
    public async Task<List<Subject>> SearchSubjectsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllSubjectsAsync();

        var lowerSearch = searchTerm.ToLower();
        return await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Where(s => !s.IsDeleted && (
                s.SubjectCode!.ToLower().Contains(lowerSearch) ||
                s.SubjectName!.ToLower().Contains(lowerSearch) ||
                s.GradeLevel!.ToLower().Contains(lowerSearch) ||
                s.Section!.ToLower().Contains(lowerSearch) ||
                (s.AcademicYear != null && s.AcademicYear.ToLower().Contains(lowerSearch)) ||
                (s.Teacher != null && (
                    s.Teacher.FirstName!.ToLower().Contains(lowerSearch) ||
                    s.Teacher.LastName!.ToLower().Contains(lowerSearch)
                ))
            ))
            .ToListAsync();
    }

    public async Task<int> ArchiveSubjectsByAcademicYearAsync(string academicYear)
    {
        if (string.IsNullOrWhiteSpace(academicYear))
        {
            return 0;
        }

        var subjectIds = await _context.Subjects
            .Where(s => !s.IsDeleted && s.AcademicYear == academicYear)
            .Select(s => s.SubjectId)
            .ToListAsync();

        var archivedCount = 0;

        foreach (var id in subjectIds)
        {
            if (await SoftDeleteSubjectAsync(id))
            {
                archivedCount++;
            }
        }

        return archivedCount;
    }
}