using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("GRADES")]
public class Grade
{
    [Key]
    [Column("grade_id")]
    public int GradeId { get; set; }

    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("teacher_id")]
    public int TeacherId { get; set; }

    [Column("subject_id")]
    public int SubjectId { get; set; }

    [Column("grade_value", TypeName = "decimal(5, 2)")]
    public decimal? GradeValue { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("EnrollmentId")]
    public virtual Enrollment? Enrollment { get; set; }

    [ForeignKey("StudentId")]
    public virtual Student? Student { get; set; }

    [ForeignKey("TeacherId")]
    public virtual Teacher? Teacher { get; set; }

    [ForeignKey("SubjectId")]
    public virtual Subject? Subject { get; set; }
}
