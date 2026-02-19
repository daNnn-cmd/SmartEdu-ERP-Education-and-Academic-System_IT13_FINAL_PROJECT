using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("APPLICANT")]
public class Applicant
{
    [Key]
    [Column("applicant_id")]
    public int ApplicantId { get; set; }

    [Required]
    [Column("job_posting_id")]
    public int JobPostingId { get; set; }

    [ForeignKey("JobPostingId")]
    public JobPosting? JobPosting { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(50)]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    [Column("resume_path")]
    public string? ResumePath { get; set; }

    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "New"; // New, Shortlisted, Hired, Rejected

    [Column("applied_date")]
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
