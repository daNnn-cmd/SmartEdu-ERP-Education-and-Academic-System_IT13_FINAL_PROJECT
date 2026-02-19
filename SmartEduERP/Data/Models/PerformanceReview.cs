using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("PERFORMANCE_REVIEW")]
public class PerformanceReview
{
    [Key]
    [Column("performance_review_id")]
    public int PerformanceReviewId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee? Employee { get; set; }

    [Column("review_date")]
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    [Column("reviewer_name")]
    public string? ReviewerName { get; set; }

    [Range(1, 5)]
    [Column("score")]
    public int Score { get; set; }

    [Column("comments")]
    public string? Comments { get; set; }

    [Column("period_start")]
    public DateTime? PeriodStart { get; set; }

    [Column("period_end")]
    public DateTime? PeriodEnd { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
