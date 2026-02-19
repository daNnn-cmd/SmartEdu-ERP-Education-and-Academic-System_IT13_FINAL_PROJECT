using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("LEAVE_REQUEST")]
public class LeaveRequest
{
    [Key]
    [Column("leave_request_id")]
    public int LeaveRequestId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee? Employee { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [MaxLength(50)]
    [Column("leave_type")]
    public string LeaveType { get; set; } = "Vacation"; // Vacation, Sick, etc.

    [Column("reason")]
    public string? Reason { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    [MaxLength(200)]
    [Column("approver_name")]
    public string? ApproverName { get; set; }

    [Column("response_notes")]
    public string? ResponseNotes { get; set; }

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
