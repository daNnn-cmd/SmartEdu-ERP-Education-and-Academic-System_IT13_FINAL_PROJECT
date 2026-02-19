using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class AnnouncementService
{
    private readonly SmartEduDbContext _context;

    public AnnouncementService(SmartEduDbContext context)
    {
        _context = context;
    }

    public async Task<List<Announcement>> GetAllAnnouncementsAsync()
    {
        return await _context.Announcements
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Announcement?> GetAnnouncementByIdAsync(int id)
    {
        return await _context.Announcements
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.AnnouncementId == id);
    }

    public async Task<List<Announcement>> GetAnnouncementsBySubjectIdAsync(int subjectId)
    {
        return await _context.Announcements
            .Include(a => a.Teacher)
            .Where(a => a.SubjectId == subjectId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Announcement>> GetAnnouncementsByTeacherIdAsync(int teacherId)
    {
        return await _context.Announcements
            .Include(a => a.Subject)
            .Where(a => a.TeacherId == teacherId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Announcement>> GetAnnouncementsForStudentAsync(int studentId)
    {
        // Get student's enrolled subjects
        var enrolledSubjectIds = await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.SubjectId)
            .ToListAsync();

        return await _context.Announcements
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Where(a => enrolledSubjectIds.Contains(a.SubjectId) && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadAnnouncementsCountBySubjectAsync(int subjectId)
    {
        // For now, return count of recent announcements (last 7 days)
        var weekAgo = DateTime.Now.AddDays(-7);
        return await _context.Announcements
            .Where(a => a.SubjectId == subjectId && a.IsActive && a.CreatedAt >= weekAgo)
            .CountAsync();
    }

    public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        announcement.CreatedAt = DateTime.Now;
        announcement.IsActive = true;
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();
        return announcement;
    }

    public async Task UpdateAnnouncementAsync(Announcement announcement)
    {
        _context.Announcements.Update(announcement);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAnnouncementAsync(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement != null)
        {
            announcement.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
