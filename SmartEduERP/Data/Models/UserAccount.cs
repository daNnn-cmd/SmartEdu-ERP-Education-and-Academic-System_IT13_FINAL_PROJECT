using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("USER_ACCOUNT")]
public class UserAccount
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [Column("first_name")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    [RegularExpression(@"^[A-Z][a-zA-Z\s\-']*$", ErrorMessage = "First name must start with a capital letter and contain only letters, spaces, hyphens, and apostrophes.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [Column("last_name")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    [RegularExpression(@"^[A-Z][a-zA-Z\s\-']*$", ErrorMessage = "Last name must start with a capital letter and contain only letters, spaces, hyphens, and apostrophes.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [Column("username")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9_.-]{2,49}$", ErrorMessage = "Username must start with a letter and can contain letters, numbers, dots, hyphens, and underscores.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Column("email")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [Column("password")]
    [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Column("role")]
    [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
    [RegularExpression(@"^(Admin|Teacher|Student|HR|Accounting)$", ErrorMessage = "Invalid role specified.")]
    public string Role { get; set; } = string.Empty;

    [Column("reference_id")]
    public int? ReferenceId { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("is_email_confirmed")]
    public bool IsEmailConfirmed { get; set; } = false;

    [Column("email_confirmation_token")]
    [MaxLength(255)]
    public string? EmailConfirmationToken { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
