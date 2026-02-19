using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("PAYMENT")]
public class Payment
{
    [Key]
    [Column("payment_id")]
    public int PaymentId { get; set; }

    [Required(ErrorMessage = "Student is required")]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Payment Date is required")]
    [Column("payment_date")]
    public DateTime PaymentDate { get; set; }

    [Required(ErrorMessage = "Payment Method is required")]
    [Column("payment_method")]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required(ErrorMessage = "Payment Status is required")]
    [Column("payment_status")]
    [MaxLength(50)]
    public string PaymentStatus { get; set; } = string.Empty;

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("StudentId")]
    public virtual Student? Student { get; set; }
}