using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("EMPLOYEE")]
public class Employee
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("middle_name")]
    public string? MiddleName { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(50)]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    [Column("department")]
    public string? Department { get; set; }

    [MaxLength(100)]
    [Column("position")]
    public string? Position { get; set; }

    [Column("hire_date")]
    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(255)]
    [Column("address")]
    public string? Address { get; set; }

    [MaxLength(50)]
    [Column("employment_status")]
    public string EmploymentStatus { get; set; } = "Active"; // Active, Inactive, OnLeave, Resigned

    [MaxLength(255)]
    [Column("resume_path")]
    public string? ResumePath { get; set; }

    [MaxLength(255)]
    [Column("contract_path")]
    public string? ContractPath { get; set; }

    [MaxLength(255)]
    [Column("id_document_path")]
    public string? IdDocumentPath { get; set; }

    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
