using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Data;

public class SmartEduDbContext : DbContext
{
    public SmartEduDbContext(DbContextOptions<SmartEduDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Accounting module entities (shared schema with AccountingDbContext)
    public DbSet<TeacherIncome> TeacherIncomes { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<Allowance> Allowances { get; set; }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<Applicant> Applicants { get; set; }
    public DbSet<EmployeeAttendance> EmployeeAttendances { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<PerformanceReview> PerformanceReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserAccount
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("USER_ACCOUNT");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id").ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.IsEmailConfirmed).HasColumnName("is_email_confirmed");
            entity.Property(e => e.EmailConfirmationToken).HasColumnName("email_confirmation_token");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("STUDENT");
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.StudentId).HasColumnName("student_id").ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.MiddleName).HasColumnName("middle_name");
            entity.Property(e => e.Suffix).HasColumnName("suffix");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.ContactNumber).HasColumnName("contact_number");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.EnrollmentStatus).HasColumnName("enrollment_status");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.IsEmailConfirmed).HasColumnName("is_email_confirmed");
            entity.Property(e => e.EmailConfirmationToken).HasColumnName("email_confirmation_token");
            entity.Property(e => e.Form137FilePath).HasColumnName("form137_file_path");
            entity.Property(e => e.PsaBirthCertFilePath).HasColumnName("psa_birth_cert_file_path");
            entity.Property(e => e.GoodMoralCertFilePath).HasColumnName("good_moral_cert_file_path");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
        });

        // Configure Teacher
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("TEACHER");
            entity.HasKey(e => e.TeacherId);
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id").ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Department).HasColumnName("department");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
        });

        // Configure Subject
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("SUBJECT");
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.SubjectId).HasColumnName("subject_id").ValueGeneratedOnAdd();
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.SubjectCode).HasColumnName("subject_code");
            entity.Property(e => e.SubjectName).HasColumnName("subject_name");
            entity.Property(e => e.GradeLevel).HasColumnName("grade_level").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Section).HasColumnName("section").IsRequired().HasMaxLength(50);
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year").IsRequired().HasMaxLength(20);
            entity.Property(e => e.SubjectStatus).HasColumnName("subject_status").IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProposedByUserId).HasColumnName("proposed_by_user_id");
            entity.Property(e => e.ProposedBy).HasColumnName("proposed_by").HasMaxLength(150);
            entity.Property(e => e.ProposedAt).HasColumnName("proposed_at");
            entity.Property(e => e.NotedByUserId).HasColumnName("noted_by_user_id");
            entity.Property(e => e.NotedBy).HasColumnName("noted_by").HasMaxLength(150);
            entity.Property(e => e.NotedAt).HasColumnName("noted_at");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by").HasMaxLength(150);
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

            entity.HasOne(e => e.Teacher)
                .WithMany(e => e.Subjects)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ProposedByUser)
                .WithMany()
                .HasForeignKey(e => e.ProposedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.NotedByUser)
                .WithMany()
                .HasForeignKey(e => e.NotedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("ENROLLMENT");
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id").ValueGeneratedOnAdd();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            entity.Property(e => e.Semester).HasColumnName("semester");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
            entity.Property(e => e.EnrollmentStatus).HasColumnName("enrollment_status").HasMaxLength(20);
            entity.Property(e => e.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by").HasMaxLength(150);
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.RejectedByUserId).HasColumnName("rejected_by_user_id");
            entity.Property(e => e.RejectedBy).HasColumnName("rejected_by").HasMaxLength(150);
            entity.Property(e => e.RejectedAt).HasColumnName("rejected_at");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

            entity.HasOne(e => e.Student)
                .WithMany(e => e.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subject)
                .WithMany(e => e.Enrollments)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.RejectedByUser)
                .WithMany()
                .HasForeignKey(e => e.RejectedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Grade
        modelBuilder.Entity<Grade>(entity =>
        {
            entity.ToTable("GRADES");
            entity.HasKey(e => e.GradeId);
            entity.Property(e => e.GradeId).HasColumnName("grade_id").ValueGeneratedOnAdd();
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.GradeValue).HasColumnName("grade_value");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

            entity.HasOne(e => e.Enrollment)
                .WithMany(e => e.Grades)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany(e => e.Grades)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Teacher)
                .WithMany(e => e.Grades)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Subject)
                .WithMany(e => e.Grades)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("PAYMENT");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id").ValueGeneratedOnAdd();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

            entity.HasOne(e => e.Student)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Submission
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.Property(e => e.SubmissionId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Activity)
                .WithMany(e => e.Submissions)
                .HasForeignKey(e => e.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Announcement
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.Property(e => e.AnnouncementId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Attendance
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.Property(e => e.AttendanceId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Global query filters for soft delete
        modelBuilder.Entity<UserAccount>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Teacher>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Subject>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Grade>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<JobPosting>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Applicant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmployeeAttendance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PerformanceReview>().HasQueryFilter(e => !e.IsDeleted);
    }
}