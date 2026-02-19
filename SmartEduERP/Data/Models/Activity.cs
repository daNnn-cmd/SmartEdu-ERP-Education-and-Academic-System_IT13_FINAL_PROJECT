using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

public class Activity
{
    [Key]
    public int ActivityId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [ForeignKey("SubjectId")]
    public Subject? Subject { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [ForeignKey("TeacherId")]
    public Teacher? Teacher { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActivityType { get; set; } = ""; // Assignment, Quiz, Project, Exam

    public int MaxScore { get; set; } = 100;

    public DateTime DueDate { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Closed

    // Navigation property
    public ICollection<Submission>? Submissions { get; set; }
}
