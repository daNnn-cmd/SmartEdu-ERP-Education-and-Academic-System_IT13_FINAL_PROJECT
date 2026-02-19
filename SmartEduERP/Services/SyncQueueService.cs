using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;
using System.Text.Json;

namespace SmartEduERP.Services;

public interface ISyncQueueService
{
    Task QueueOperationAsync(string operationType, string tableName, int recordId, object? data, object? oldData = null, CancellationToken cancellationToken = default);
    Task<List<SyncQueue>> GetPendingSyncOperationsAsync(CancellationToken cancellationToken = default);
    Task MarkAsSyncedAsync(int syncQueueId, CancellationToken cancellationToken = default);
    Task ClearSyncedOperationsAsync(CancellationToken cancellationToken = default);
    Task<int> GetPendingOperationCountAsync(CancellationToken cancellationToken = default);
}

public class SyncQueueService : ISyncQueueService
{
    private readonly SmartEduDbContext _context;
    private readonly ILogger<SyncQueueService> _logger;

    public SyncQueueService(SmartEduDbContext context, ILogger<SyncQueueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task QueueOperationAsync(string operationType, string tableName, int recordId, object? data, object? oldData = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueItem = new SyncQueue
            {
                OperationType = operationType,
                TableName = tableName,
                RecordId = recordId,
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                OldData = oldData != null ? JsonSerializer.Serialize(oldData) : null,
                CreatedAt = DateTime.UtcNow,
                IsSynced = false,
                SyncError = null
            };

            await _context.Set<SyncQueue>().AddAsync(queueItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error queueing sync operation {operationType} for {tableName} (ID: {recordId})");
            throw;
        }
    }

    public async Task<List<SyncQueue>> GetPendingSyncOperationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SyncQueue>()
            .Where(q => !q.IsSynced)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsSyncedAsync(int syncQueueId, CancellationToken cancellationToken = default)
    {
        var item = await _context.Set<SyncQueue>().FirstOrDefaultAsync(q => q.SyncQueueId == syncQueueId, cancellationToken);
        if (item == null)
        {
            return;
        }

        item.IsSynced = true;
        item.SyncedAt = DateTime.UtcNow;
        item.SyncError = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearSyncedOperationsAsync(CancellationToken cancellationToken = default)
    {
        var syncedItems = await _context.Set<SyncQueue>()
            .Where(q => q.IsSynced)
            .ToListAsync(cancellationToken);

        if (syncedItems.Count == 0)
        {
            return;
        }

        _context.Set<SyncQueue>().RemoveRange(syncedItems);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetPendingOperationCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SyncQueue>()
            .CountAsync(q => !q.IsSynced, cancellationToken);
    }
}
