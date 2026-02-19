using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("Allowance")]
public class Allowance
{
    [Key]
    [Column("AllowanceId")]
    public int AllowanceId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Description")]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Allowance amount must be greater than 0")]
    [Column("Amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column("IsPercentage")]
    public bool IsPercentage { get; set; }

    [Column("IsTaxable")]
    public bool IsTaxable { get; set; } = true;

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; }

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }
}
