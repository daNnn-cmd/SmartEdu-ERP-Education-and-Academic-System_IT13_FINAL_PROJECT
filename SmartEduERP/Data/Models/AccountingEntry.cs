using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("AccountingEntry")]
public class AccountingEntry
{
    [Key]
    [Column("AccountingEntryId")]
    public int AccountingEntryId { get; set; }

    [Column("TeacherIncomeId")]
    public int? TeacherIncomeId { get; set; }

    [Required]
    [Column("EntryDate")]
    public DateTime EntryDate { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("DebitAccount")]
    public string DebitAccount { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("CreditAccount")]
    public string CreditAccount { get; set; } = string.Empty;

    [Column("Amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(50)]
    [Column("EntryType")]
    public string? EntryType { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; }

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("TeacherIncomeId")]
    public virtual TeacherIncome? TeacherIncome { get; set; }
}
