using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("SUBJECT")]
public class Subject
{
    [Key]
    [Column("subject_id")]
    public int SubjectId { get; set; }

    [Column("teacher_id")]
    public int? TeacherId { get; set; }

    [Required(ErrorMessage = "Subject Code is required")]
    [Column("subject_code")]
    [MaxLength(50)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject Name is required")]
    [Column("subject_name")]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grade Level is required")]
    [Column("grade_level")]
    [MaxLength(50)]
    public string GradeLevel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Section is required")]
    [Column("section")]
    [MaxLength(50)]
    public string Section { get; set; } = string.Empty;

    [Required(ErrorMessage = "School Year is required")]
    [Column("academic_year")]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Proposal/Approval Workflow Fields
    [Column("subject_status")]
    [MaxLength(20)]
    public string SubjectStatus { get; set; } = "Proposed";

    [Column("proposed_by_user_id")]
    public int? ProposedByUserId { get; set; }

    [Column("proposed_by")]
    [MaxLength(150)]
    public string? ProposedBy { get; set; }

    [Column("proposed_at")]
    public DateTime? ProposedAt { get; set; }

    [Column("noted_by_user_id")]
    public int? NotedByUserId { get; set; }

    [Column("noted_by")]
    [MaxLength(150)]
    public string? NotedBy { get; set; }

    [Column("noted_at")]
    public DateTime? NotedAt { get; set; }

    [Column("approved_by_user_id")]
    public int? ApprovedByUserId { get; set; }

    [Column("approved_by")]
    [MaxLength(150)]
    public string? ApprovedBy { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    [ForeignKey("TeacherId")]
    public virtual Teacher? Teacher { get; set; }

    [ForeignKey("ProposedByUserId")]
    public virtual UserAccount? ProposedByUser { get; set; }

    [ForeignKey("NotedByUserId")]
    public virtual UserAccount? NotedByUser { get; set; }

    [ForeignKey("ApprovedByUserId")]
    public virtual UserAccount? ApprovedByUser { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}