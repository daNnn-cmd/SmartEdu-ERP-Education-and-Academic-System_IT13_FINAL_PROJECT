using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("Tax")]
public class Tax
{
    [Key]
    [Column("TaxId")]
    public int TaxId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Description")]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Tax rate must be greater than 0")]
    [Column("Rate", TypeName = "decimal(18,4)")]
    public decimal Rate { get; set; }

    [Column("IsPercentage")]
    public bool IsPercentage { get; set; } = true;

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
