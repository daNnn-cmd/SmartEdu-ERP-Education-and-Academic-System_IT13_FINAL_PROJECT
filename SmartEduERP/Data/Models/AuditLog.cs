using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models
{
    [Table("AUDIT_LOGS")]
    public class AuditLog
    {
        [Key]
        [Column("audit_id")]
        public int AuditId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("table_name")]
        public string TableName { get; set; } = string.Empty;

        [Column("record_id")]
        public int RecordId { get; set; }

        [Column("old_values")]
        public string? OldValues { get; set; }

        [Column("new_values")]
        public string? NewValues { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to UserAccount
        [ForeignKey("UserId")]
        public virtual UserAccount? User { get; set; }

        // Add Username to be explicitly selected in queries
        [NotMapped]
        public string? Username { get; set; }

        // Computed property for display
        [NotMapped]
        public string UserDisplayName => User != null ? $"{User.FirstName} {User.LastName}" : (Username ?? "System");
    }
}