using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("ENROLLMENT")]
public class Enrollment
{
    [Key]
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    [Column("student_id")]
    [Range(1, int.MaxValue, ErrorMessage = "Student is required.")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [Column("subject_id")]
    [Range(1, int.MaxValue, ErrorMessage = "Subject is required.")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Academic year is required.")]
    [Column("academic_year")]
    [MaxLength(20, ErrorMessage = "Academic year cannot exceed 20 characters.")]
    public string AcademicYear { get; set; } = string.Empty;

    [Required(ErrorMessage = "Semester is required.")]
    [Column("semester")]
    [MaxLength(20, ErrorMessage = "Semester cannot exceed 20 characters.")]
    public string Semester { get; set; } = string.Empty;

    [Column("enrollment_date")] // This is the main enrollment date
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [Column("enrollment_status")]
    [MaxLength(20)]
    public string EnrollmentStatus { get; set; } = "Pending";

    [Column("approved_by_user_id")]
    public int? ApprovedByUserId { get; set; }

    [Column("approved_by")]
    [MaxLength(150)]
    public string? ApprovedBy { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("rejected_by_user_id")]
    public int? RejectedByUserId { get; set; }

    [Column("rejected_by")]
    [MaxLength(150)]
    public string? RejectedBy { get; set; }

    [Column("rejected_at")]
    public DateTime? RejectedAt { get; set; }

    [Column("rejection_reason")]
    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("StudentId")]
    public virtual Student? Student { get; set; }

    [ForeignKey("SubjectId")]
    public virtual Subject? Subject { get; set; }

    [ForeignKey("ApprovedByUserId")]
    public virtual UserAccount? ApprovedByUser { get; set; }

    [ForeignKey("RejectedByUserId")]
    public virtual UserAccount? RejectedByUser { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}