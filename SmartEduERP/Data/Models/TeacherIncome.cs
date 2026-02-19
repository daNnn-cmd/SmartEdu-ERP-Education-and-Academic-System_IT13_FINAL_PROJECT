using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("TeacherIncome")]
public class TeacherIncome
{
    [Key]
    [Column("TeacherIncomeId")]
    public int TeacherIncomeId { get; set; }

    [Required]
    [Column("TeacherId")]
    public int TeacherId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("TeacherName")]
    public string TeacherName { get; set; } = string.Empty;

    [Required]
    [Column("PeriodStartDate")]
    public DateTime PeriodStartDate { get; set; }

    [Required]
    [Column("PeriodEndDate")]
    public DateTime PeriodEndDate { get; set; }

    [Required]
    [Column("BasicSalary", TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    [Column("OvertimePay", TypeName = "decimal(18,2)")]
    public decimal OvertimePay { get; set; }

    [Column("OtherIncome", TypeName = "decimal(18,2)")]
    public decimal OtherIncome { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; }

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AccountingEntry> Entries { get; set; } = new List<AccountingEntry>();
}
