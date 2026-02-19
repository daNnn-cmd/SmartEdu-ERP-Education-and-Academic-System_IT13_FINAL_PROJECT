using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class ActivityService
{
    private readonly SmartEduDbContext _context;

    public ActivityService(SmartEduDbContext context)
    {
        _context = context;
    }

    public async Task<List<Activity>> GetAllActivitiesAsync()
    {
        return await _context.Activities
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Activity?> GetActivityByIdAsync(int id)
    {
        return await _context.Activities
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.ActivityId == id);
    }

    public async Task<List<Activity>> GetActivitiesByTeacherIdAsync(int teacherId)
    {
        return await _context.Activities
            .Include(a => a.Subject)
            .Include(a => a.Submissions)
            .Where(a => a.TeacherId == teacherId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Activity>> GetActivitiesBySubjectIdAsync(int subjectId)
    {
        return await _context.Activities
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .Where(a => a.SubjectId == subjectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Activity>> GetActivitiesForStudentAsync(int studentId)
    {
        // Get student's enrolled subjects
        var enrolledSubjectIds = await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.SubjectId)
            .ToListAsync();

        return await _context.Activities
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .Where(a => enrolledSubjectIds.Contains(a.SubjectId))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        activity.CreatedAt = DateTime.Now;
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task UpdateActivityAsync(Activity activity)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteActivityAsync(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalActivitiesCountAsync()
    {
        return await _context.Activities.CountAsync();
    }

    public async Task<int> GetActiveActivitiesCountAsync()
    {
        return await _context.Activities
            .Where(a => a.Status == "Active")
            .CountAsync();
    }
}
