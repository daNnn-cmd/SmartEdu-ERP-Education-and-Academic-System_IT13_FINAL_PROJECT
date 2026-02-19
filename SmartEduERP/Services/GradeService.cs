using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class GradeService
{
    private readonly SmartEduDbContext _context;
    private readonly ISyncQueueService _syncQueueService;
    private readonly AuditLogService _auditLogService;

    public GradeService(SmartEduDbContext context, ISyncQueueService syncQueueService, AuditLogService auditLogService)
    {
        _context = context;
        _syncQueueService = syncQueueService;
        _auditLogService = auditLogService;
    }

    public async Task<List<Grade>> GetAllGradesAsync()
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .ToListAsync();
    }

    public async Task<Grade?> GetGradeByIdAsync(int id)
    {
        return await _context.Grades
            .Include(g => g.Enrollment)
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .FirstOrDefaultAsync(g => g.GradeId == id);
    }

    public async Task<Grade> CreateGradeAsync(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();

        try
        {
            var newGradeJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                grade.GradeId,
                grade.StudentId,
                grade.SubjectId,
                grade.TeacherId,
                grade.EnrollmentId,
                grade.GradeValue,
                grade.Remarks
            });

            await _auditLogService.LogActionAsync(
                action: "Create",
                tableName: "Grades",
                recordId: grade.GradeId,
                userId: null,
                oldValues: null,
                newValues: newGradeJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging grade create audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Create",
                "Grades",
                grade.GradeId,
                new
                {
                    grade.GradeId,
                    grade.StudentId,
                    grade.SubjectId,
                    grade.TeacherId,
                    grade.EnrollmentId,
                    grade.GradeValue,
                    grade.Remarks
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing grade create sync: {ex.Message}");
        }
        return grade;
    }

    public async Task<Grade?> UpdateGradeAsync(Grade grade)
    {
        var existing = await _context.Grades.FindAsync(grade.GradeId);
        if (existing == null)
            return null;

        var oldGradeJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            existing.GradeId,
            existing.StudentId,
            existing.SubjectId,
            existing.TeacherId,
            existing.EnrollmentId,
            existing.GradeValue,
            existing.Remarks
        });

        existing.EnrollmentId = grade.EnrollmentId;
        existing.StudentId = grade.StudentId;
        existing.TeacherId = grade.TeacherId;
        existing.SubjectId = grade.SubjectId;
        existing.GradeValue = grade.GradeValue;
        existing.Remarks = grade.Remarks;

        await _context.SaveChangesAsync();

        try
        {
            var newGradeJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                existing.GradeId,
                existing.StudentId,
                existing.SubjectId,
                existing.TeacherId,
                existing.EnrollmentId,
                existing.GradeValue,
                existing.Remarks
            });

            await _auditLogService.LogActionAsync(
                action: "Update",
                tableName: "Grades",
                recordId: existing.GradeId,
                userId: null,
                oldValues: oldGradeJson,
                newValues: newGradeJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging grade update audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Grades",
                existing.GradeId,
                new
                {
                    existing.GradeId,
                    existing.StudentId,
                    existing.SubjectId,
                    existing.TeacherId,
                    existing.EnrollmentId,
                    existing.GradeValue,
                    existing.Remarks
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing grade update sync: {ex.Message}");
        }

        return existing;
    }

    public async Task<bool> SoftDeleteGradeAsync(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade == null)
            return false;

        var deletedGradeJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            grade.GradeId,
            grade.StudentId,
            grade.SubjectId,
            grade.TeacherId,
            grade.EnrollmentId,
            grade.GradeValue,
            grade.Remarks
        });

        grade.IsDeleted = true;
        grade.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Grades",
                recordId: grade.GradeId,
                userId: null,
                oldValues: deletedGradeJson,
                newValues: "Grade soft deleted"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging grade delete audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Delete",
                "Grades",
                grade.GradeId,
                new
                {
                    grade.GradeId,
                    grade.StudentId,
                    grade.SubjectId,
                    grade.TeacherId,
                    grade.EnrollmentId
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing grade delete sync: {ex.Message}");
        }

        return true;
    }

    public async Task<List<Grade>> GetGradesByStudentAsync(int studentId)
    {
        return await _context.Grades
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Where(g => g.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<List<Grade>> GetGradesByTeacherAsync(int teacherId)
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Where(g => g.TeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<List<Grade>> GetGradesBySubjectAsync(int subjectId)
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .Where(g => g.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<List<Grade>> GetGradesByStudentIdAsync(int studentId)
    {
        return await GetGradesByStudentAsync(studentId);
    }

    public async Task<List<Grade>> GetGradesByTeacherIdAsync(int teacherId)
    {
        return await GetGradesByTeacherAsync(teacherId);
    }

    public async Task<int> GetTotalGradeCountAsync()
    {
        return await _context.Grades.CountAsync();
    }
}
