using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class AttendanceService
{
    private readonly SmartEduDbContext _context;

    public AttendanceService(SmartEduDbContext context)
    {
        _context = context;
    }

    public async Task<List<Attendance>> GetAllAttendanceAsync()
    {
        return await _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Subject)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        return await _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.AttendanceId == id);
    }

    public async Task<List<Attendance>> GetAttendanceByStudentIdAsync(int studentId)
    {
        return await _context.Attendances
            .Include(a => a.Subject)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetAttendanceBySubjectIdAsync(int subjectId)
    {
        return await _context.Attendances
            .Include(a => a.Student)
            .Where(a => a.SubjectId == subjectId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetAttendanceByDateRangeAsync(int studentId, DateTime startDate, DateTime endDate)
    {
        return await _context.Attendances
            .Include(a => a.Subject)
            .Where(a => a.StudentId == studentId && a.Date >= startDate && a.Date <= endDate)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        attendance.CreatedAt = DateTime.Now;
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task UpdateAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAttendanceAsync(int id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance != null)
        {
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<string, int>> GetAttendanceStatsByStudentAsync(int studentId, string period = "all")
    {
        var query = _context.Attendances.Where(a => a.StudentId == studentId);

        // Filter by period
        switch (period.ToLower())
        {
            case "daily":
                query = query.Where(a => a.Date.Date == DateTime.Today);
                break;
            case "weekly":
                var weekAgo = DateTime.Today.AddDays(-7);
                query = query.Where(a => a.Date >= weekAgo);
                break;
            case "monthly":
                var monthAgo = DateTime.Today.AddMonths(-1);
                query = query.Where(a => a.Date >= monthAgo);
                break;
        }

        var attendances = await query.ToListAsync();

        return new Dictionary<string, int>
        {
            { "Total", attendances.Count },
            { "Present", attendances.Count(a => a.Status == "Present") },
            { "Absent", attendances.Count(a => a.Status == "Absent") },
            { "Late", attendances.Count(a => a.Status == "Late") },
            { "Excused", attendances.Count(a => a.Status == "Excused") }
        };
    }

    public async Task<double> GetAttendanceRateAsync(int studentId, string period = "all")
    {
        var stats = await GetAttendanceStatsByStudentAsync(studentId, period);
        if (stats["Total"] == 0) return 0;
        
        var presentCount = stats["Present"] + stats["Late"];
        return Math.Round((double)presentCount / stats["Total"] * 100, 2);
    }
}
