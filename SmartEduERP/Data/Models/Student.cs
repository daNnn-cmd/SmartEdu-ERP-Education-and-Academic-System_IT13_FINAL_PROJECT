using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models
{
    [Table("STUDENT")]
    public class Student
    {
        [Key]
        [Column("student_id")]
        public int StudentId { get; set; }

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

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Column("email")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid contact number.")]
        [Column("contact_number")]
        [MaxLength(20, ErrorMessage = "Contact number cannot exceed 20 characters.")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact number must be 11 digits starting with 09.")]
        public string? ContactNumber { get; set; } = "";

        [Column("address")]
        [MaxLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; } = "";

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [Column("date_of_birth")]
        [CustomValidation(typeof(Student), nameof(ValidateDateOfBirth))]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Enrollment status is required.")]
        [Column("enrollment_status")]
        [MaxLength(50, ErrorMessage = "Enrollment status cannot exceed 50 characters.")]
        public string? EnrollmentStatus { get; set; } = "Active";

        [Column("middle_name")]
        [MaxLength(100, ErrorMessage = "Middle name cannot exceed 100 characters.")]
        [RegularExpression(@"^[A-Z][a-zA-Z\s\-']*$", ErrorMessage = "Middle name must start with a capital letter and contain only letters, spaces, hyphens, and apostrophes.")]
        public string? MiddleName { get; set; }

        [Column("suffix")]
        [MaxLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Column("gender")]
        [MaxLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
        public string? Gender { get; set; } = "";

        [Column("form137_file_path")]
        [MaxLength(500, ErrorMessage = "Form 137 file path cannot exceed 500 characters.")]
        public string? Form137FilePath { get; set; }

        [Column("psa_birth_cert_file_path")]
        [MaxLength(500, ErrorMessage = "PSA birth certificate file path cannot exceed 500 characters.")]
        public string? PsaBirthCertFilePath { get; set; }

        [Column("good_moral_cert_file_path")]
        [MaxLength(500, ErrorMessage = "Good moral certificate file path cannot exceed 500 characters.")]
        public string? GoodMoralCertFilePath { get; set; }

        [Column("registration_date")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationDate { get; set; } = DateTime.Now;

        [Column("is_email_confirmed")]
        public bool IsEmailConfirmed { get; set; } = false;

        [Column("email_confirmation_token")]
        [MaxLength(255, ErrorMessage = "Email confirmation token cannot exceed 255 characters.")]
        public string? EmailConfirmationToken { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("DeletedAt")]
        public DateTime? DeletedAt { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Custom validation for date of birth
        public static ValidationResult ValidateDateOfBirth(DateTime? dateOfBirth, ValidationContext context)
        {
            if (!dateOfBirth.HasValue)
            {
                return new ValidationResult("Date of birth is required.");
            }

            if (dateOfBirth.Value > DateTime.Now)
            {
                return new ValidationResult("Date of birth cannot be in the future.");
            }

            var age = DateTime.Now.Year - dateOfBirth.Value.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.Value.DayOfYear)
                age--;

            if (age <= 4)
            {
                return new ValidationResult("Student must be older than 4 years.");
            }

            if (dateOfBirth.Value < DateTime.Now.AddYears(-100))
            {
                return new ValidationResult("Date of birth seems invalid. Please check the year.");
            }

            return ValidationResult.Success!;
        }

        public Student()
        {
            Enrollments = new HashSet<Enrollment>();
            Grades = new HashSet<Grade>();
            Payments = new HashSet<Payment>();
        }

        // Navigation properties
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Grade> Grades { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}