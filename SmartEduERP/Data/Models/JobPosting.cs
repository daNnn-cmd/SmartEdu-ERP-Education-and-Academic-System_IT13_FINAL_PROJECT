using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("JOB_POSTING")]
public class JobPosting
{
    [Key]
    [Column("job_posting_id")]
    public int JobPostingId { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("department")]
    public string? Department { get; set; }

    [MaxLength(50)]
    [Column("employment_type")]
    public string? EmploymentType { get; set; } // Full-time, Part-time, Contract

    [Column("description")]
    public string? Description { get; set; }

    [Column("posted_date")]
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;

    [Column("closing_date")]
    public DateTime? ClosingDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Applicant> Applicants { get; set; } = new List<Applicant>();
}
