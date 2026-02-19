// Services/GlobalSyncService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartEduERP.Data;
using System.Linq;

namespace SmartEduERP.Services
{
    public interface IGlobalSyncService
    {
        Task<bool> SyncAllChangesAsync();
        Task<bool> SyncTableAsync<T>(string tableName) where T : class;
        Task<int> GetPendingSyncCountAsync();
        Task<SyncStatus> GetSyncStatusAsync();
    }

    public class GlobalSyncService : IGlobalSyncService
    {
        private readonly IBidirectionalSyncService _bidirectionalSyncService;
        private readonly IDbContextFactory<SmartEduDbContext> _dbContextFactory;
        private readonly ILogger<GlobalSyncService> _logger;
        private readonly IConfiguration _configuration;

        public GlobalSyncService(
            IBidirectionalSyncService bidirectionalSyncService,
            IDbContextFactory<SmartEduDbContext> dbContextFactory,
            ILogger<GlobalSyncService> logger,
            IConfiguration configuration)
        {
            _bidirectionalSyncService = bidirectionalSyncService;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SyncAllChangesAsync()
        {
            try
            {
                _logger.LogInformation("🔄 Starting global bidirectional sync of all changes...");

                var result = await _bidirectionalSyncService.SyncAsync();

                if (result.Success)
                {
                    _logger.LogInformation($"✅ Global bidirectional sync completed successfully. Added: {result.RecordsAdded}, Updated: {result.RecordsUpdated}, Deleted: {result.RecordsDeleted}");
                }
                else
                {
                    var errorSummary = result.Errors != null && result.Errors.Any()
                        ? string.Join(", ", result.Errors)
                        : "Unknown error";

                    _logger.LogWarning($"⚠️ Global bidirectional sync completed with some issues: {errorSummary}");
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Global sync failed");

                return false;
            }
        }

        public async Task<bool> SyncTableAsync<T>(string tableName) where T : class
        {
            try
            {
                _logger.LogInformation($"🔄 Syncing table {tableName} via bidirectional sync (all tables)...");

                var result = await _bidirectionalSyncService.SyncAsync();

                if (!result.Success)
                {
                    var errorSummary = result.Errors != null && result.Errors.Any()
                        ? string.Join(", ", result.Errors)
                        : "Unknown error";

                    _logger.LogWarning($"⚠️ Table sync for {tableName} completed with errors: {errorSummary}");
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Table sync failed for {tableName}");

                return false;
            }
        }

        public async Task<int> GetPendingSyncCountAsync()
        {
            try
            {
                using var localDb = await _dbContextFactory.CreateDbContextAsync();
                using var cloudDb = await _dbContextFactory.CreateDbContextAsync();

                // Set cloud connection
                var cloudConnection = _configuration.GetConnectionString("CloudConnection");
                if (string.IsNullOrEmpty(cloudConnection))
                {
                    _logger.LogWarning("⚠️ Cloud connection string not found");
                    return 0;
                }

                cloudDb.Database.SetConnectionString(cloudConnection);

                // Test cloud connection first
                var cloudConnected = await cloudDb.Database.CanConnectAsync();
                if (!cloudConnected)
                {
                    _logger.LogWarning("⚠️ Cloud database not accessible for sync count");
                    return 0;
                }

                // Compare counts for each table
                int totalPending = 0;

                // Students
                var localStudents = await localDb.Students.CountAsync();
                var cloudStudents = await cloudDb.Students.CountAsync();
                totalPending += Math.Abs(localStudents - cloudStudents);

                // Teachers
                var localTeachers = await localDb.Teachers.CountAsync();
                var cloudTeachers = await cloudDb.Teachers.CountAsync();
                totalPending += Math.Abs(localTeachers - cloudTeachers);

                // UserAccounts
                var localUsers = await localDb.UserAccounts.CountAsync();
                var cloudUsers = await cloudDb.UserAccounts.CountAsync();
                totalPending += Math.Abs(localUsers - cloudUsers);

                // Payments
                var localPayments = await localDb.Payments.CountAsync();
                var cloudPayments = await cloudDb.Payments.CountAsync();
                totalPending += Math.Abs(localPayments - cloudPayments);

                // HR - Employees
                var localEmployees = await localDb.Employees.CountAsync();
                var cloudEmployees = await cloudDb.Employees.CountAsync();
                totalPending += Math.Abs(localEmployees - cloudEmployees);

                // HR - Job Postings
                var localJobPostings = await localDb.JobPostings.CountAsync();
                var cloudJobPostings = await cloudDb.JobPostings.CountAsync();
                totalPending += Math.Abs(localJobPostings - cloudJobPostings);

                // HR - Applicants
                var localApplicants = await localDb.Applicants.CountAsync();
                var cloudApplicants = await cloudDb.Applicants.CountAsync();
                totalPending += Math.Abs(localApplicants - cloudApplicants);

                // HR - Employee Attendance
                var localEmployeeAttendance = await localDb.EmployeeAttendances.CountAsync();
                var cloudEmployeeAttendance = await cloudDb.EmployeeAttendances.CountAsync();
                totalPending += Math.Abs(localEmployeeAttendance - cloudEmployeeAttendance);

                // HR - Leave Requests
                var localLeaveRequests = await localDb.LeaveRequests.CountAsync();
                var cloudLeaveRequests = await cloudDb.LeaveRequests.CountAsync();
                totalPending += Math.Abs(localLeaveRequests - cloudLeaveRequests);

                // HR - Performance Reviews
                var localPerformanceReviews = await localDb.PerformanceReviews.CountAsync();
                var cloudPerformanceReviews = await cloudDb.PerformanceReviews.CountAsync();
                totalPending += Math.Abs(localPerformanceReviews - cloudPerformanceReviews);

                _logger.LogInformation($"📊 Sync status: {totalPending} pending changes across all tables");

                return totalPending;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calculating pending sync count");
                return 0;
            }
        }

        public async Task<SyncStatus> GetSyncStatusAsync()
        {
            var status = new SyncStatus();

            try
            {
                using var localDb = await _dbContextFactory.CreateDbContextAsync();
                using var cloudDb = await _dbContextFactory.CreateDbContextAsync();

                var cloudConnection = _configuration.GetConnectionString("CloudConnection");
                if (!string.IsNullOrEmpty(cloudConnection))
                {
                    cloudDb.Database.SetConnectionString(cloudConnection);
                    status.CloudAccessible = await cloudDb.Database.CanConnectAsync();
                }

                status.PendingSyncCount = await GetPendingSyncCountAsync();
                status.LastSyncAttempt = DateTime.Now;
                status.Status = status.PendingSyncCount == 0 ? "Synced" : "Pending";

                _logger.LogInformation($"📊 Sync Status: {status.Status}, Pending: {status.PendingSyncCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting sync status");
                status.Status = "Error";
            }

            return status;
        }
    }

    public class SyncStatus
    {
        public int PendingSyncCount { get; set; }
        public DateTime LastSyncAttempt { get; set; }
        public string Status { get; set; } = "Unknown";
        public bool CloudAccessible { get; set; }
    }
}