using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("EMPLOYEE_ATTENDANCE")]
public class EmployeeAttendance
{
    [Key]
    [Column("attendance_id")]
    public int EmployeeAttendanceId { get; set; }

    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee? Employee { get; set; }

    [Column("date")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Column("time_in")]
    public TimeSpan? TimeIn { get; set; }

    [Column("time_out")]
    public TimeSpan? TimeOut { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Present"; // Present, Late, Absent

    [Column("minutes_late")]
    public int MinutesLate { get; set; }

    [Column("is_absent")]
    public bool IsAbsent { get; set; }

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
}
