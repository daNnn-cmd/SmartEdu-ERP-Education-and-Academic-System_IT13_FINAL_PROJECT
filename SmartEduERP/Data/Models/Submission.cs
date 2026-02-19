using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

public class Submission
{
    [Key]
    public int SubmissionId { get; set; }

    [Required]
    public int ActivityId { get; set; }

    [ForeignKey("ActivityId")]
    public Activity? Activity { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey("StudentId")]
    public Student? Student { get; set; }

    public DateTime? SubmittedAt { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Submitted, Late, Graded

    public string? SubmissionContent { get; set; }

    public string? Attachments { get; set; }

    public int? Score { get; set; }

    public string? Feedback { get; set; }

    public DateTime? GradedAt { get; set; }
}
