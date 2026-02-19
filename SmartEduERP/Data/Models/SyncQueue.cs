using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEduERP.Data.Models;

[Table("SYNC_QUEUE")]
public class SyncQueue
{
    [Key]
    [Column("sync_queue_id")]
    public int SyncQueueId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("operation_type")]
    public string OperationType { get; set; } = string.Empty; // Create, Update, Delete

    [Required]
    [MaxLength(100)]
    [Column("table_name")]
    public string TableName { get; set; } = string.Empty; // Students, Teachers, etc.

    [Column("record_id")]
    public int RecordId { get; set; }

    [Column("data")]
    public string? Data { get; set; } // JSON serialized data

    [Column("old_data")]
    public string? OldData { get; set; } // JSON serialized old data for updates

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("synced_at")]
    public DateTime? SyncedAt { get; set; }

    [Column("is_synced")]
    public bool IsSynced { get; set; } = false;

    [Column("sync_error")]
    public string? SyncError { get; set; }
}
