using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class HrService
{
    private readonly SmartEduDbContext _context;

    public HrService(SmartEduDbContext context)
    {
        _context = context;
    }

    // EMPLOYEES
    public async Task<List<Employee>> GetEmployeesAsync()
    {
        return await _context.Employees
            .OrderByDescending(e => e.CreatedAt)
            .ThenByDescending(e => e.EmployeeId)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task<Employee> SaveEmployeeAsync(Employee employee)
    {
        if (employee.EmployeeId == 0)
        {
            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;
            _context.Employees.Add(employee);
        }
        else
        {
            employee.UpdatedAt = DateTime.UtcNow;
            _context.Employees.Update(employee);
        }

        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> SoftDeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalEmployeesAsync()
    {
        return await _context.Employees.CountAsync();
    }

    // RECRUITMENT
    public async Task<List<JobPosting>> GetJobPostingsAsync()
    {
        return await _context.JobPostings
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<JobPosting> SaveJobPostingAsync(JobPosting posting)
    {
        if (posting.JobPostingId == 0)
        {
            posting.PostedDate = DateTime.UtcNow;
            posting.CreatedAt = DateTime.UtcNow;
            posting.UpdatedAt = DateTime.UtcNow;
            _context.JobPostings.Add(posting);
        }
        else
        {
            posting.UpdatedAt = DateTime.UtcNow;
            _context.JobPostings.Update(posting);
        }

        await _context.SaveChangesAsync();
        return posting;
    }

    public async Task<int> GetOpenJobPostingsCountAsync()
    {
        return await _context.JobPostings.CountAsync(j => j.IsActive);
    }

    public async Task<List<Applicant>> GetApplicantsAsync()
    {
        return await _context.Applicants
            .Include(a => a.JobPosting)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    public async Task<Applicant> AddApplicantAsync(Applicant applicant)
    {
        applicant.AppliedDate = DateTime.UtcNow;
        applicant.CreatedAt = DateTime.UtcNow;
        applicant.UpdatedAt = DateTime.UtcNow;
        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();
        return applicant;
    }

    public async Task UpdateApplicantStatusAsync(int applicantId, string status)
    {
        var applicant = await _context.Applicants.FindAsync(applicantId);
        if (applicant == null) return;

        // Do not change status if applicant is already in a final state
        if (applicant.Status == "Shortlisted" ||
            applicant.Status == "Hired" ||
            applicant.Status == "Rejected")
        {
            return;
        }

        applicant.Status = status;
        applicant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ATTENDANCE
    public async Task<List<EmployeeAttendance>> GetAttendanceAsync(DateTime? date = null)
    {
        var query = _context.EmployeeAttendances
            .Include(a => a.Employee)
            .AsQueryable();

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(a => a.Date.Date == targetDate);
        }

        return await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee!.LastName)
            .ToListAsync();
    }

    public async Task<EmployeeAttendance> SaveAttendanceAsync(EmployeeAttendance attendance)
    {
        // Business rules: compute status, late minutes, absence
        var workStart = new TimeSpan(9, 0, 0); // 9:00 AM standard start

        if (!attendance.TimeIn.HasValue && !attendance.TimeOut.HasValue)
        {
            attendance.Status = "Absent";
            attendance.IsAbsent = true;
            attendance.MinutesLate = 0;
        }
        else
        {
            if (attendance.TimeIn.HasValue && attendance.TimeIn.Value > workStart)
            {
                attendance.MinutesLate = (int)(attendance.TimeIn.Value - workStart).TotalMinutes;
                attendance.Status = attendance.MinutesLate > 0 ? "Late" : "Present";
            }
            else
            {
                attendance.MinutesLate = 0;
                attendance.Status = "Present";
            }

            attendance.IsAbsent = false;
        }

        attendance.Date = attendance.Date.Date;
        attendance.UpdatedAt = DateTime.UtcNow;

        if (attendance.EmployeeAttendanceId == 0)
        {
            attendance.CreatedAt = DateTime.UtcNow;
            _context.EmployeeAttendances.Add(attendance);
        }
        else
        {
            _context.EmployeeAttendances.Update(attendance);
        }

        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<(int total, int present, int late, int absent)> GetAttendanceSummaryAsync(DateTime date)
    {
        var targetDate = date.Date;
        var records = await _context.EmployeeAttendances
            .Where(a => a.Date.Date == targetDate)
            .ToListAsync();

        var total = records.Count;
        var present = records.Count(a => a.Status == "Present");
        var late = records.Count(a => a.Status == "Late");
        var absent = records.Count(a => a.Status == "Absent");

        return (total, present, late, absent);
    }

    // LEAVE REQUESTS
    public async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .OrderByDescending(l => l.RequestedAt)
            .ToListAsync();
    }

    public async Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequest request)
    {
        request.RequestedAt = DateTime.UtcNow;
        request.Status = "Pending";
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<bool> UpdateLeaveStatusAsync(int leaveRequestId, string status, string? approverName, string? responseNotes)
    {
        var leave = await _context.LeaveRequests.FindAsync(leaveRequestId);
        if (leave == null) return false;

        leave.Status = status;
        leave.ApproverName = approverName;
        leave.ResponseNotes = responseNotes;
        leave.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // PERFORMANCE REVIEWS
    public async Task<List<PerformanceReview>> GetPerformanceReviewsAsync()
    {
        return await _context.PerformanceReviews
            .Include(p => p.Employee)
            .OrderByDescending(p => p.ReviewDate)
            .ToListAsync();
    }

    public async Task<PerformanceReview> SavePerformanceReviewAsync(PerformanceReview review)
    {
        review.UpdatedAt = DateTime.UtcNow;

        if (review.PerformanceReviewId == 0)
        {
            review.CreatedAt = DateTime.UtcNow;
            _context.PerformanceReviews.Add(review);
        }
        else
        {
            _context.PerformanceReviews.Update(review);
        }

        await _context.SaveChangesAsync();
        return review;
    }

    // PAYROLL INTEGRATION
    public async Task<List<PayrollAttendanceExportDto>> GetPayrollAttendanceAsync(DateTime periodStart, DateTime periodEnd)
    {
        var start = periodStart.Date;
        var end = periodEnd.Date;

        var records = await _context.EmployeeAttendances
            .Include(a => a.Employee)
            .Where(a => a.Date >= start && a.Date <= end)
            .ToListAsync();

        var grouped = records
            .GroupBy(a => a.EmployeeId)
            .Select(g => new PayrollAttendanceExportDto
            {
                EmployeeId = g.Key,
                EmployeeName = g.First().Employee != null
                    ? g.First().Employee!.FirstName + " " + g.First().Employee!.LastName
                    : "Unknown",
                PeriodStart = start,
                PeriodEnd = end,
                TotalDays = g.Count(),
                PresentDays = g.Count(r => r.Status == "Present" || r.Status == "Late"),
                AbsentDays = g.Count(r => r.Status == "Absent"),
                TotalMinutesLate = g.Sum(r => r.MinutesLate)
            })
            .OrderBy(x => x.EmployeeName)
            .ToList();

        return grouped;
    }

    public class PayrollAttendanceExportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int TotalMinutesLate { get; set; }
    }
}
