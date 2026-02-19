using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("TEACHER")]
public class Teacher
{
    [Key]
    [Column("teacher_id")]
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "First Name is required")]
    [Column("first_name")]
    [MaxLength(100)]
    [RegularExpression(@"^[A-Z][a-zA-Z\s\-']*$", ErrorMessage = "First name must start with a capital letter and contain only letters, spaces, hyphens, and apostrophes.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name is required")]
    [Column("last_name")]
    [MaxLength(100)]
    [RegularExpression(@"^[A-Z][a-zA-Z\s\-']*$", ErrorMessage = "Last name must start with a capital letter and contain only letters, spaces, hyphens, and apostrophes.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    [Column("department")]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Column("position")]
    [MaxLength(100)]
    public string? Position { get; set; }

    // ADDED: Registration date
    [Column("registration_date")]
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}