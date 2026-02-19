using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SmartEduERP.Services
{
    
    public interface IBidirectionalSyncService
    {
        Task<SyncResult> SyncAsync();
        Task<BidirectionalSyncStatus> GetSyncStatusAsync();
        Task<bool> IsSyncInProgressAsync();
        Task<List<SyncConflict>> GetConflictsAsync();
        Task<bool> ResolveConflictAsync(int conflictId, ConflictResolution resolution);
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public int RecordsAdded { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsDeleted { get; set; }
        public int ConflictsResolved { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncStartTime { get; set; }
        public DateTime SyncEndTime { get; set; }
        public TimeSpan Duration => SyncEndTime - SyncStartTime;
    }

    public class BidirectionalSyncStatus
    {
        public bool IsInProgress { get; set; }
        public DateTime LastSyncTime { get; set; }
        public int PendingChanges { get; set; }
        public int PendingConflicts { get; set; }
        public string? LastSyncDirection { get; set; }
        public string? LastSyncStatus { get; set; }
    }

    public class SyncConflict
    {
        public int ConflictId { get; set; }
        public string? TableName { get; set; }
        public int RecordId { get; set; }
        public DateTime LocalUpdatedAt { get; set; }
        public DateTime CloudUpdatedAt { get; set; }
        public string? LocalData { get; set; }
        public string? CloudData { get; set; }
        public DateTime DetectedAt { get; set; }
    }

    public enum ConflictResolution
    {
        UseLocal,
        UseCloud,
        Merge
    }

    public class BidirectionalSyncService : IBidirectionalSyncService
    {
        private readonly IDbContextFactory<SmartEduDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BidirectionalSyncService> _logger;
        private readonly AuditLogService _auditLogService;
        private bool _isSyncing = false;
        private DateTime _lastSyncTime = DateTime.MinValue;
        private readonly ConcurrentDictionary<int, SyncConflict> _conflicts = new();

        public BidirectionalSyncService(
            IDbContextFactory<SmartEduDbContext> dbContextFactory,
            IConfiguration configuration,
            ILogger<BidirectionalSyncService> logger,
            AuditLogService auditLogService)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _logger = logger;
            _auditLogService = auditLogService;
        }

        public Task<bool> IsSyncInProgressAsync()
        {
            return Task.FromResult(_isSyncing);
        }

        public async Task<BidirectionalSyncStatus> GetSyncStatusAsync()
        {
            return new BidirectionalSyncStatus
            {
                IsInProgress = _isSyncing,
                LastSyncTime = _lastSyncTime,
                PendingChanges = await GetPendingChangesCountAsync(),
                PendingConflicts = _conflicts.Count,
                LastSyncStatus = _isSyncing ? "In Progress" : "Idle"
            };
        }

        public Task<List<SyncConflict>> GetConflictsAsync()
        {
            return Task.FromResult(_conflicts.Values.ToList());
        }

        public Task<bool> ResolveConflictAsync(int conflictId, ConflictResolution resolution)
        {
            if (!_conflicts.TryGetValue(conflictId, out var conflict))
            {
                _logger.LogWarning($"Conflict {conflictId} not found");
                return Task.FromResult(false);
            }

            try
            {
                _logger.LogInformation($"Resolving conflict {conflictId} using {resolution} strategy");
                _conflicts.TryRemove(conflictId, out _);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving conflict {conflictId}");
                return Task.FromResult(false);
            }
        }

        public async Task<SyncResult> SyncAsync()
        {
            if (_isSyncing)
            {
                _logger.LogWarning("Sync already in progress, skipping");
                return new SyncResult { Success = false, Errors = new() { "Sync already in progress" } };
            }

            _isSyncing = true;
            var result = new SyncResult { SyncStartTime = DateTime.UtcNow };

            try
            {
                _logger.LogInformation("🚀 Starting bidirectional sync...");

                using var localDb = await CreateLocalDbContextAsync();
                using var cloudDb = await CreateCloudDbContextAsync();

                // Test connections
                if (!await TestConnectionAsync(localDb))
                {
                    result.Success = false;
                    result.Errors.Add("❌ Local database connection failed");
                    _logger.LogError("Local database connection failed");
                    return result;
                }

                if (!await TestConnectionAsync(cloudDb))
                {
                    result.Success = false;
                    result.Errors.Add("❌ Cloud database connection failed");
                    _logger.LogError("Cloud database connection failed");
                    return result;
                }

                _logger.LogInformation("✅ Database connections verified");

                // Sync each table bidirectionally
                await SyncTableBidirectionalAsync<UserAccount>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Student>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Teacher>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Subject>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Enrollment>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Grade>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Payment>(localDb, cloudDb, result);

                // Accounting module tables
                await SyncTableBidirectionalAsync<TeacherIncome>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Tax>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Allowance>(localDb, cloudDb, result);

                // HR module tables
                await SyncTableBidirectionalAsync<Employee>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<JobPosting>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<Applicant>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<EmployeeAttendance>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<LeaveRequest>(localDb, cloudDb, result);
                await SyncTableBidirectionalAsync<PerformanceReview>(localDb, cloudDb, result);

                await SyncTableBidirectionalAsync<AuditLog>(localDb, cloudDb, result);

                result.Success = true;
                _lastSyncTime = DateTime.UtcNow;
                _logger.LogInformation($"✅ Sync completed: Added {result.RecordsAdded}, Updated {result.RecordsUpdated}, Deleted {result.RecordsDeleted}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Sync failed");
                result.Success = false;
                result.Errors.Add(ex.Message);
                if (ex.InnerException != null)
                {
                    result.Errors.Add($"Inner: {ex.InnerException.Message}");
                }
            }
            finally
            {
                result.SyncEndTime = DateTime.UtcNow;
                _isSyncing = false;
            }

            await LogSyncAuditAsync(result);

            return result;
        }

        private async Task LogSyncAuditAsync(SyncResult result)
        {
            try
            {
                var details = JsonSerializer.Serialize(new
                {
                    result.Success,
                    result.RecordsAdded,
                    result.RecordsUpdated,
                    result.RecordsDeleted,
                    result.ConflictsResolved,
                    result.Errors,
                    result.SyncStartTime,
                    result.SyncEndTime,
                    DurationSeconds = result.Duration.TotalSeconds
                });

                await _auditLogService.LogActionAsync(
                    action: "Sync",
                    tableName: "Sync",
                    recordId: 0,
                    userId: null,
                    oldValues: null,
                    newValues: details);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write sync audit log");
            }
        }

        private async Task SyncTableBidirectionalAsync<T>(
            SmartEduDbContext localDb,
            SmartEduDbContext cloudDb,
            SyncResult result) where T : class
        {
            try
            {
                var tableName = typeof(T).Name;
                _logger.LogInformation($"🔄 Syncing {tableName} bidirectionally...");

                var localSet = localDb.Set<T>();
                var cloudSet = cloudDb.Set<T>();

                // Fetch records (including soft-deleted for comparison)
                var localRecords = await localSet.IgnoreQueryFilters().AsNoTracking().ToListAsync();
                var cloudRecords = await cloudSet.IgnoreQueryFilters().AsNoTracking().ToListAsync();

                // Count active records for logging
                var localActiveCount = localRecords.Count(r => !IsSoftDeleted(r));
                var cloudActiveCount = cloudRecords.Count(r => !IsSoftDeleted(r));

                _logger.LogInformation($"📊 {tableName}: Local={localActiveCount} active (total {localRecords.Count}), Cloud={cloudActiveCount} active (total {cloudRecords.Count})");

                // Sync from local to cloud (LOCAL → CLOUD)
                var localToCloudStats = await SyncDirectionAsync(localRecords, cloudRecords, localSet, cloudSet, cloudDb, result, "Local→Cloud");

                // Refresh cloud records after first sync
                cloudRecords = await cloudSet.AsNoTracking().ToListAsync();

                // Sync from cloud to local (CLOUD → LOCAL)
                var cloudToLocalStats = await SyncDirectionAsync(cloudRecords, localRecords, cloudSet, localSet, localDb, result, "Cloud→Local");

                _logger.LogInformation($"✅ {tableName} sync completed: L→C(+{localToCloudStats.Added}~{localToCloudStats.Updated}), C→L(+{cloudToLocalStats.Added}~{cloudToLocalStats.Updated})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error syncing {typeof(T).Name}");
                result.Errors.Add($"Error syncing {typeof(T).Name}: {ex.Message}");
            }
        }

        private async Task<(int Added, int Updated)> SyncDirectionAsync<T>(
            List<T> sourceRecords,
            List<T> destinationRecords,
            DbSet<T> sourceSet,
            DbSet<T> destinationSet,
            SmartEduDbContext destinationDb,
            SyncResult result,
            string direction) where T : class
        {
            int recordsAdded = 0;
            int recordsUpdated = 0;

            var isTeacherType = typeof(T) == typeof(Teacher);
            var isLocalToCloud = direction == "Local→Cloud";
            var isCloudToLocal = direction == "Cloud→Local";

            try
            {
                foreach (var sourceRecord in sourceRecords)
                {
                    try
                    {
                        // Determine the primary key property using EF metadata first
                        var entityType = destinationDb.Model.FindEntityType(typeof(T));
                        var primaryKey = entityType?.FindPrimaryKey();
                        System.Reflection.PropertyInfo? idProperty = null;

                        if (primaryKey != null && primaryKey.Properties.Count > 0)
                        {
                            var keyPropName = primaryKey.Properties[0].Name;
                            idProperty = typeof(T).GetProperty(keyPropName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        }

                        // Fallbacks for common Id naming patterns (case-insensitive)
                        idProperty = idProperty
                                     ?? typeof(T).GetProperty("Id", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? typeof(T).GetProperty($"{typeof(T).Name}Id", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

                        if (idProperty == null)
                        {
                            _logger.LogWarning($"  ⚠️ Skipped {typeof(T).Name} - no Id property found. Available properties: {string.Join(", ", typeof(T).GetProperties().Select(p => p.Name))}");
                            continue;
                        }

                        var sourceId = idProperty.GetValue(sourceRecord);
                        if (sourceId == null)
                        {
                            _logger.LogWarning($"  ⚠️ Skipped {typeof(T).Name} - null Id value");
                            continue;
                        }

                        // Find matching record by ID
                        var existingRecord = destinationRecords.FirstOrDefault(r =>
                            idProperty.GetValue(r)?.Equals(sourceId) ?? false);

                        if (existingRecord == null)
                        {
                            // Record exists in source but not in destination - add it
                            var detachedRecord = CreateDetachedCopy(sourceRecord);
                            if (detachedRecord != null)
                            {
                                try
                                {
                                    destinationSet.Add(detachedRecord);
                                    recordsAdded++;
                                    result.RecordsAdded++;
                                    _logger.LogInformation($"  ➕ Added {typeof(T).Name} (ID: {sourceId}) via {direction}");
                                }
                                catch (Exception addEx)
                                {
                                    _logger.LogError(addEx, $"Failed to add {typeof(T).Name} (ID: {sourceId}): {addEx.Message}");
                                    result.Errors.Add($"Failed to add {typeof(T).Name} (ID: {sourceId}): {addEx.Message}");
                                }
                            }
                        }
                        else
                        {
                            // Record exists in both - check for conflicts using timestamps

                            if (isTeacherType && isLocalToCloud)
                            {
                                var detachedRecord = CreateDetachedCopy(sourceRecord);
                                if (detachedRecord != null)
                                {
                                    T? tracked = null;
                                    try
                                    {
                                        var findResult = await destinationSet.FindAsync(sourceId);
                                        tracked = findResult as T;
                                    }
                                    catch
                                    {
                                    }

                                    if (tracked != null)
                                    {
                                        destinationDb.Entry(tracked).CurrentValues.SetValues(detachedRecord);
                                        destinationDb.Entry(tracked).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        var entry = destinationDb.Attach(existingRecord);
                                        entry.CurrentValues.SetValues(detachedRecord);
                                        entry.State = EntityState.Modified;
                                    }

                                    recordsUpdated++;
                                    result.RecordsUpdated++;
                                    _logger.LogInformation($"  🔄 Updated {typeof(T).Name} (ID: {sourceId}) via {direction} (local is source of truth)");
                                }
                            }
                            else if (isTeacherType && isCloudToLocal)
                            {
                                _logger.LogInformation($"  ⏭️  Skipped {typeof(T).Name} (ID: {sourceId}) via {direction} because local is source of truth");
                            }
                            else
                            {
                                var sourceUpdated = GetUpdatedDate(sourceRecord);
                                var destUpdated = GetUpdatedDate(existingRecord);

                                if (sourceUpdated > destUpdated)
                                {
                                    // Source is newer - update destination
                                    var detachedRecord = CreateDetachedCopy(sourceRecord);
                                    if (detachedRecord != null)
                                    {
                                        // Ensure we're updating a tracked entity so EF persists changes
                                        T? tracked = null;
                                        try
                                        {
                                            // Attempt to load tracked entity by key (single-key assumption)
                                            var findResult = await destinationSet.FindAsync(sourceId);
                                            tracked = findResult as T;
                                        }
                                        catch
                                        {
                                        }

                                        if (tracked != null)
                                        {
                                            destinationDb.Entry(tracked).CurrentValues.SetValues(detachedRecord);
                                            destinationDb.Entry(tracked).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            // Attach existing record and mark modified
                                            var entry = destinationDb.Attach(existingRecord);
                                            entry.CurrentValues.SetValues(detachedRecord);
                                            entry.State = EntityState.Modified;
                                        }

                                        recordsUpdated++;
                                        result.RecordsUpdated++;
                                        _logger.LogInformation($"  🔄 Updated {typeof(T).Name} (ID: {sourceId}) via {direction}");
                                    }
                                }
                                else if (sourceUpdated < destUpdated)
                                {
                                    // Destination is newer - skip
                                    _logger.LogInformation($"  ⏭️  Skipped {typeof(T).Name} (ID: {sourceId}) - destination is newer");
                                }
                                // If equal, no update needed
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error syncing record in {direction}");
                        result.Errors.Add($"Error syncing record: {ex.Message}");
                    }
                }

                // Save all changes once, enabling IDENTITY_INSERT when adding explicit keys
                try
                {
                    if (recordsAdded > 0)
                    {
                        var entityTypeMeta = destinationDb.Model.FindEntityType(typeof(T));
                        var efTableName = entityTypeMeta?.GetTableName();
                        var efSchema = entityTypeMeta?.GetSchema();
                        var fullName = string.IsNullOrEmpty(efSchema) ? $"[{efTableName}]" : $"[{efSchema}].[{efTableName}]";

                        try
                        {
                            await destinationDb.Database.OpenConnectionAsync();
                            await destinationDb.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {fullName} ON");
                        }
                        catch (Exception onEx)
                        {
                            _logger.LogDebug(onEx, $"IDENTITY_INSERT ON skipped for {fullName} (may not be identity table)");
                        }

                        try
                        {
                            var changes = await destinationDb.SaveChangesAsync();
                            if (changes > 0)
                            {
                                _logger.LogInformation($"  💾 Saved {changes} changes to {typeof(T).Name} (IDENTITY_INSERT)");
                            }
                        }
                        finally
                        {
                            try
                            {
                                await destinationDb.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {fullName} OFF");
                            }
                            catch { /* ignore */ }
                        }
                    }
                    else
                    {
                        var changes = await destinationDb.SaveChangesAsync();
                        if (changes > 0)
                        {
                            _logger.LogInformation($"  💾 Saved {changes} changes to {typeof(T).Name}");
                        }
                        else if (recordsAdded > 0 || recordsUpdated > 0)
                        {
                            _logger.LogWarning($"  ⚠️ {recordsAdded + recordsUpdated} records were marked for {typeof(T).Name} but SaveChangesAsync returned 0. This may indicate a database issue.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error saving changes for {typeof(T).Name}: {ex.Message}");
                    result.Errors.Add($"Error saving {typeof(T).Name}: {ex.Message}");
                    
                    // Log inner exception for more details
                    if (ex.InnerException != null)
                    {
                        _logger.LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                        result.Errors.Add($"Inner error: {ex.InnerException.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in sync direction {direction}");
                result.Errors.Add($"Sync direction error: {ex.Message}");
            }

            return (recordsAdded, recordsUpdated);
        }

        private T? CreateDetachedCopy<T>(T entity) where T : class
        {
            if (entity == null) return null;

            try
            {
                // Use reflection to create a new instance and copy all properties
                var type = typeof(T);
                var newInstance = Activator.CreateInstance(type) as T;
                
                if (newInstance == null) return entity;

                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if (property.CanRead && property.CanWrite)
                    {
                        try
                        {
                            var value = property.GetValue(entity);
                            property.SetValue(newInstance, value);
                        }
                        catch
                        {
                            // Skip properties that can't be copied
                        }
                    }
                }

                return newInstance;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to create detached copy of {typeof(T).Name}, using original");
                return entity;
            }
        }

        private DateTime GetUpdatedDate<T>(T entity) where T : class
        {
            try
            {
                var updatedProperty = typeof(T).GetProperty("UpdatedAt");
                if (updatedProperty != null && updatedProperty.GetValue(entity) is DateTime updatedDate)
                {
                    return updatedDate;
                }

                var createdProperty = typeof(T).GetProperty("CreatedAt");
                if (createdProperty != null && createdProperty.GetValue(entity) is DateTime createdDate)
                {
                    return createdDate;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error getting updated date for {typeof(T).Name}");
            }

            return DateTime.MinValue;
        }

        private object[] GetKeyValues<T>(T entity, SmartEduDbContext context) where T : class
        {
            try
            {
                var entityType = context.Model.FindEntityType(typeof(T));
                if (entityType?.FindPrimaryKey() == null)
                {
                    _logger.LogWarning($"No primary key found for {typeof(T).Name}");
                    return Array.Empty<object>();
                }

                var primaryKey = entityType?.FindPrimaryKey();
                if (primaryKey == null)
                {
                    return Array.Empty<object>();
                }
                if (primaryKey == null)
                {
                    return Array.Empty<object>();
                }

                var keyProperties = primaryKey.Properties;
                var keyValues = new object[keyProperties.Count];

                for (int i = 0; i < keyProperties.Count; i++)
                {
                    var propertyInfo = keyProperties[i].PropertyInfo;
                    if (propertyInfo != null)
                    {
                        keyValues[i] = propertyInfo.GetValue(entity) ?? 0;
                    }
                }

                return keyValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting key values for {typeof(T).Name}");
                return Array.Empty<object>();
            }
        }

        private Task<int> GetPendingChangesCountAsync()
        {
            // This would track pending changes - for now return 0
            return Task.FromResult(0);
        }

        private async Task<SmartEduDbContext> CreateLocalDbContextAsync()
        {
            try
            {
                var context = await _dbContextFactory.CreateDbContextAsync();
                var localConnection = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(localConnection))
                {
                    throw new InvalidOperationException("Local connection string is not configured");
                }

                context.Database.SetConnectionString(localConnection);
                _logger.LogInformation($"🔌 Local DB: {GetMaskedConnectionString(localConnection)}");
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating local DB context");
                throw;
            }
        }

        private async Task<SmartEduDbContext> CreateCloudDbContextAsync()
        {
            try
            {
                var context = await _dbContextFactory.CreateDbContextAsync();
                var cloudConnection = _configuration.GetConnectionString("CloudConnection");

                if (string.IsNullOrEmpty(cloudConnection))
                {
                    throw new InvalidOperationException("Cloud connection string is not configured");
                }

                context.Database.SetConnectionString(cloudConnection);
                _logger.LogInformation($"🔌 Cloud DB: {GetMaskedConnectionString(cloudConnection)}");
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cloud DB context");
                throw;
            }
        }

        private async Task<bool> TestConnectionAsync(SmartEduDbContext context)
        {
            try
            {
                return await context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return false;
            }
        }

        private bool IsSoftDeleted<T>(T entity) where T : class
        {
            try
            {
                var isDeletedProperty = typeof(T).GetProperty("IsDeleted", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
                if (isDeletedProperty != null && isDeletedProperty.GetValue(entity) is bool isDeleted)
                {
                    return isDeleted;
                }
            }
            catch
            {
                // If we can't determine, assume not deleted
            }
            return false;
        }

        private string GetMaskedConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return "Empty";

            try
            {
                var parts = connectionString.Split(';');
                var maskedParts = parts.Select(part =>
                {
                    if (part.Trim().StartsWith("Password", StringComparison.OrdinalIgnoreCase) ||
                        part.Trim().StartsWith("Pwd", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Password=***";
                    }
                    return part;
                });
                return string.Join(";", maskedParts);
            }
            catch
            {
                return "Invalid connection string";
            }
        }
    }
}
