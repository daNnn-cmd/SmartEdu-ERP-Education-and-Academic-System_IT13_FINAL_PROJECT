using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class SubmissionService
{
    private readonly SmartEduDbContext _context;

    public SubmissionService(SmartEduDbContext context)
    {
        _context = context;
    }

    public async Task<List<Submission>> GetAllSubmissionsAsync()
    {
        return await _context.Submissions
            .Include(s => s.Activity)
            .Include(s => s.Student)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Submission?> GetSubmissionByIdAsync(int id)
    {
        return await _context.Submissions
            .Include(s => s.Activity)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.SubmissionId == id);
    }

    public async Task<List<Submission>> GetSubmissionsByActivityIdAsync(int activityId)
    {
        return await _context.Submissions
            .Include(s => s.Student)
            .Where(s => s.ActivityId == activityId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<Submission>> GetSubmissionsByStudentIdAsync(int studentId)
    {
        return await _context.Submissions
            .Include(s => s.Activity)
                .ThenInclude(a => a!.Subject)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Submission?> GetSubmissionByActivityAndStudentAsync(int activityId, int studentId)
    {
        return await _context.Submissions
            .Include(s => s.Activity)
            .FirstOrDefaultAsync(s => s.ActivityId == activityId && s.StudentId == studentId);
    }

    public async Task<Submission> CreateSubmissionAsync(Submission submission)
    {
        submission.SubmittedAt = DateTime.Now;
        
        // Check if submission is late
        var activity = await _context.Activities.FindAsync(submission.ActivityId);
        if (activity != null && submission.SubmittedAt > activity.DueDate)
        {
            submission.Status = "Late";
        }
        else
        {
            submission.Status = "Submitted";
        }

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task UpdateSubmissionAsync(Submission submission)
    {
        _context.Submissions.Update(submission);
        await _context.SaveChangesAsync();
    }

    public async Task GradeSubmissionAsync(int submissionId, int score, string? feedback)
    {
        var submission = await _context.Submissions.FindAsync(submissionId);
        if (submission != null)
        {
            submission.Score = score;
            submission.Feedback = feedback;
            submission.Status = "Graded";
            submission.GradedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteSubmissionAsync(int id)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission != null)
        {
            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<string, int>> GetSubmissionStatsByStudentAsync(int studentId)
    {
        var submissions = await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .ToListAsync();

        return new Dictionary<string, int>
        {
            { "Total", submissions.Count },
            { "Submitted", submissions.Count(s => s.Status == "Submitted") },
            { "Late", submissions.Count(s => s.Status == "Late") },
            { "Graded", submissions.Count(s => s.Status == "Graded") },
            { "Pending", submissions.Count(s => s.Status == "Pending") }
        };
    }
}
