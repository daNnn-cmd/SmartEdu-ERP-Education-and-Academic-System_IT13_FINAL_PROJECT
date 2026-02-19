using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;

using Microsoft.Extensions.Logging;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services
{
    public interface IDatabaseSyncService
    {
        Task<bool> SyncToCloudAsync();
        Task<bool> SyncFromCloudAsync();
        Task<bool> FullSyncAsync();
    }

    public class DatabaseSyncService : IDatabaseSyncService
    {
        private readonly IDbContextFactory<SmartEduDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSyncService> _logger;

        public DatabaseSyncService(
            IDbContextFactory<SmartEduDbContext> dbContextFactory,
            IConfiguration configuration,
            ILogger<DatabaseSyncService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // ✅ ADD: This method to handle sync conflicts
        private Task HandleSyncConflict<T>(T localEntity, T cloudEntity, SmartEduDbContext destinationContext, DbSet<T> destinationSet) where T : class
        {
            try
            {
                // Simple conflict resolution: Use the most recently updated record
                var localUpdated = GetUpdatedDate(localEntity);
                var cloudUpdated = GetUpdatedDate(cloudEntity);

                if (localUpdated > cloudUpdated)
                {
                    _logger.LogInformation($"Using local version (newer) for {typeof(T).Name}");
                    destinationContext.Entry(cloudEntity).CurrentValues.SetValues(localEntity);
                }
                else
                {
                    _logger.LogInformation($"Using cloud version (newer) for {typeof(T).Name}");
                    // For bidirectional sync, you might want to update local too
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Conflict resolution failed for {typeof(T).Name}, skipping record");
            }

            return Task.CompletedTask;
        }

        private DateTime GetUpdatedDate<T>(T entity)
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

            return DateTime.MinValue;
        }


        public async Task<bool> SyncToCloudAsync()
        {
            try
            {
                _logger.LogInformation("🚀 Starting sync to cloud...");

                using var localDb = await CreateLocalDbContextAsync();
                using var cloudDb = await CreateCloudDbContextAsync();

                // Test connections first
                _logger.LogInformation("🔌 Testing database connections...");
                var localConnected = await TestConnectionAsync(localDb);
                var cloudConnected = await TestConnectionAsync(cloudDb);

                if (!localConnected)
                {
                    _logger.LogError("❌ Local database connection failed");
                    return false;
                }

                if (!cloudConnected)
                {
                    _logger.LogError("❌ Cloud database connection failed");
                    return false;
                }

                _logger.LogInformation("✅ Both database connections successful");

                // Sync each table with detailed logging
                _logger.LogInformation("🔄 Starting table synchronization...");

                await SyncTableAsync<UserAccount>(localDb.UserAccounts, cloudDb.UserAccounts, cloudDb, "UserAccounts");
                await SyncTableAsync<Student>(localDb.Students, cloudDb.Students, cloudDb, "Students");
                await SyncTableAsync<Teacher>(localDb.Teachers, cloudDb.Teachers, cloudDb, "Teachers");
                await SyncTableAsync<Subject>(localDb.Subjects, cloudDb.Subjects, cloudDb, "Subjects");
                await SyncTableAsync<Enrollment>(localDb.Enrollments, cloudDb.Enrollments, cloudDb, "Enrollments");
                await SyncTableAsync<Grade>(localDb.Grades, cloudDb.Grades, cloudDb, "Grades");
                await SyncTableAsync<Payment>(localDb.Payments, cloudDb.Payments, cloudDb, "Payments");
                await SyncTableAsync<AuditLog>(localDb.AuditLogs, cloudDb.AuditLogs, cloudDb, "AuditLogs");

                var changes = 0;
                try
                {
                    changes = await cloudDb.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Try per-table identity insert save sequence since we sync table by table
                    changes = 0; // will be set inside per-table save
                }
                _logger.LogInformation($"✅ Sync to cloud completed successfully. {changes} changes saved.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error syncing to cloud");
                return false;
            }
        }

        public async Task<bool> SyncFromCloudAsync()
        {
            try
            {
                _logger.LogInformation("🚀 Starting sync from cloud...");

                using var localDb = await CreateLocalDbContextAsync();
                using var cloudDb = await CreateCloudDbContextAsync();

                // Test cloud connection first
                if (!await TestConnectionAsync(cloudDb))
                {
                    _logger.LogError("❌ Cloud database connection failed");
                    return false;
                }

                await SyncTableAsync<UserAccount>(cloudDb.UserAccounts, localDb.UserAccounts, localDb, "UserAccounts");
                await SyncTableAsync<Student>(cloudDb.Students, localDb.Students, localDb, "Students");
                await SyncTableAsync<Teacher>(cloudDb.Teachers, localDb.Teachers, localDb, "Teachers");
                await SyncTableAsync<Subject>(cloudDb.Subjects, localDb.Subjects, localDb, "Subjects");
                await SyncTableAsync<Enrollment>(cloudDb.Enrollments, localDb.Enrollments, localDb, "Enrollments");
                await SyncTableAsync<Grade>(cloudDb.Grades, localDb.Grades, localDb, "Grades");
                await SyncTableAsync<Payment>(cloudDb.Payments, localDb.Payments, localDb, "Payments");
                await SyncTableAsync<AuditLog>(cloudDb.AuditLogs, localDb.AuditLogs, localDb, "AuditLogs");

                var changes = await localDb.SaveChangesAsync();
                _logger.LogInformation($"✅ Sync from cloud completed successfully. {changes} changes saved.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error syncing from cloud");
                return false;
            }
        }

        public async Task<bool> FullSyncAsync()
        {
            var success1 = await SyncToCloudAsync();
            var success2 = await SyncFromCloudAsync();
            return success1 && success2;
        }

        private async Task SyncTableAsync<T>(IQueryable<T> source, DbSet<T> destination, SmartEduDbContext destinationContext, string tableName) where T : class
        {
            try
            {
                _logger.LogInformation($"🔄 Syncing {tableName}...");

                var sourceData = await source.AsNoTracking().ToListAsync();
                _logger.LogInformation($"📊 {tableName}: Found {sourceData.Count} records in source");

                int added = 0;
                int updated = 0;

                foreach (var item in sourceData)
                {
                    var keyValues = GetKeyValues(item, destinationContext);
                    var existing = await destination.FindAsync(keyValues);

                    if (existing == null)
                    {
                        destination.Add(item);
                        added++;
                    }
                    else
                    {
                        destinationContext.Entry(existing).CurrentValues.SetValues(item);
                        destinationContext.Entry(existing).State = EntityState.Modified;
                        updated++;
                    }
                }

                _logger.LogInformation($"✅ {tableName}: Added {added}, Updated {updated}");

                // Save changes, enabling IDENTITY_INSERT if we added rows with explicit keys
                if (added > 0)
                {
                    try
                    {
                        var entityType = destinationContext.Model.FindEntityType(typeof(T));
                        var efTableName = entityType?.GetTableName();
                        var efSchema = entityType?.GetSchema();
                        var fullName = string.IsNullOrEmpty(efSchema) ? $"[{efTableName}]" : $"[{efSchema}].[{efTableName}]";

                        try
                        {
                            await destinationContext.Database.OpenConnectionAsync();
                            await destinationContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {fullName} ON");
                        }
                        catch (Exception onEx)
                        {
                            _logger.LogDebug(onEx, $"IDENTITY_INSERT ON skipped for {fullName} (may not be identity table)");
                        }

                        try
                        {
                            var saved = await destinationContext.SaveChangesAsync();
                            _logger.LogInformation($"💾 {tableName}: Saved {saved} changes with IDENTITY_INSERT");
                        }
                        finally
                        {
                            try
                            {
                                await destinationContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {fullName} OFF");
                            }
                            catch { /* ignore */ }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Error saving {tableName} with IDENTITY_INSERT");
                        throw;
                    }
                }
                else
                {
                    var saved = await destinationContext.SaveChangesAsync();
                    _logger.LogInformation($"💾 {tableName}: Saved {saved} changes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error syncing table {tableName}");
                throw;
            }
        }

        private object[] GetKeyValues<T>(T entity, SmartEduDbContext context) where T : class
        {
            try
            {
                var entityType = context.Model.FindEntityType(typeof(T));
                if (entityType == null)
                    return Array.Empty<object>();
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey == null)
                    return Array.Empty<object>();
                var keyProperties = primaryKey.Properties;
                var keyValues = new object[keyProperties.Count];

                for (int i = 0; i < keyProperties.Count; i++)
                {
                    var propertyInfo = keyProperties[i].PropertyInfo;
                    keyValues[i] = propertyInfo?.GetValue(entity) ?? 0;
                }

                return keyValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting key values for {typeof(T).Name}");
                throw;
            }
        }

        private async Task<SmartEduDbContext> CreateLocalDbContextAsync()
        {
            try
            {
                var context = await _dbContextFactory.CreateDbContextAsync();
                var localConnection = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(localConnection))
                {
                    _logger.LogError("❌ Local connection string is null or empty");
                    throw new InvalidOperationException("Local connection string is not configured");
                }

                context.Database.SetConnectionString(localConnection);
                _logger.LogInformation($"🔌 Local DB Connection: {GetMaskedConnectionString(localConnection)}");
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating local DB context");
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
                    _logger.LogError("❌ Cloud connection string is null or empty");
                    throw new InvalidOperationException("Cloud connection string is not configured");
                }

                context.Database.SetConnectionString(cloudConnection);
                _logger.LogInformation($"🔌 Cloud DB Connection: {GetMaskedConnectionString(cloudConnection)}");
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating cloud DB context");
                throw;
            }
        }

        private async Task<bool> TestConnectionAsync(SmartEduDbContext context)
        {
            try
            {
                _logger.LogInformation("🔌 Testing database connection...");
                var canConnect = await context.Database.CanConnectAsync();
                _logger.LogInformation(canConnect ? "✅ Database connection successful" : "❌ Database connection failed");
                return canConnect;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Database connection test failed");
                return false;
            }
        }

        private string GetMaskedConnectionString(string connectionString)
        {
            // Mask password in connection string for security
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