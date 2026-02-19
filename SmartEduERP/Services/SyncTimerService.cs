using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Timers;

namespace SmartEduERP.Services
{
    public class SyncTimerService : IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SyncTimerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConnectivityService _connectivityService;
        private System.Timers.Timer? _syncTimer;
        private bool _isSyncing = false;

        public SyncTimerService(IServiceProvider services, ILogger<SyncTimerService> logger, IConfiguration configuration, IConnectivityService connectivityService)
        {
            _services = services;
            _logger = logger;
            _configuration = configuration;
            _connectivityService = connectivityService;

            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            _connectivityService.StartMonitoring();

            // ✅ FIX: Auto-start the timer in constructor for MAUI
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            var syncIntervalMinutes = _configuration.GetValue<int>("DatabaseSync:SyncIntervalMinutes", 5);
            var syncEnabled = _configuration.GetValue<bool>("DatabaseSync:SyncEnabled", true);
            var syncOnStartup = _configuration.GetValue<bool>("DatabaseSync:SyncOnStartup", true);

            _logger.LogInformation($"Database sync initialized - Enabled: {syncEnabled}, Interval: {syncIntervalMinutes} minutes, SyncOnStartup: {syncOnStartup}");

            if (syncEnabled)
            {
                // Initial sync on startup
                if (syncOnStartup)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10)); // Wait for app to start
                        await DoSync();
                    });
                }

                // Setup periodic sync timer
                _syncTimer = new System.Timers.Timer(TimeSpan.FromMinutes(syncIntervalMinutes).TotalMilliseconds);
                _syncTimer.Elapsed += OnSyncTimerElapsed;
                _syncTimer.AutoReset = true;
                _syncTimer.Start();

                _logger.LogInformation($"Automatic sync timer started with {syncIntervalMinutes} minute interval");
            }
        }

        private async void OnSyncTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isSyncing) return;

            var syncEnabled = _configuration.GetValue<bool>("DatabaseSync:SyncEnabled", true);
            if (syncEnabled)
            {
                await DoSync();
            }
        }

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                var syncOnWifiConnect = _configuration.GetValue<bool>("DatabaseSync:SyncOnWiFiConnect", false);
                if (!syncOnWifiConnect)
                {
                    return;
                }

                if (e.IsWifiConnected && e.HasInternet && !_isSyncing)
                {
                    _logger.LogInformation("📶 WiFi with internet detected. Triggering bidirectional sync.");
                    await DoSync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling connectivity change event");
            }
        }

        private async Task DoSync()
        {
            if (_isSyncing) return;

            _isSyncing = true;
            try
            {
                using var scope = _services.CreateScope();
                var bidirectionalSyncService = scope.ServiceProvider.GetRequiredService<IBidirectionalSyncService>();

                if (bidirectionalSyncService != null)
                {
                    _logger.LogInformation("🔄 Starting automatic bidirectional database sync...");
                    var result = await bidirectionalSyncService.SyncAsync();

                    if (result.Success)
                    {
                        _logger.LogInformation($"✅ Automatic bidirectional sync completed. Added: {result.RecordsAdded}, Updated: {result.RecordsUpdated}, Deleted: {result.RecordsDeleted}");
                    }
                    else
                    {
                        var errorSummary = result.Errors != null && result.Errors.Any()
                            ? string.Join(", ", result.Errors)
                            : "Unknown error";

                        _logger.LogWarning($"⚠️ Automatic bidirectional sync completed with errors: {errorSummary}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in background sync");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        // Manual sync methods that can be called from your UI
        public async Task<bool> SyncToCloudAsync()
        {
            try
            {
                using var scope = _services.CreateScope();
                var bidirectionalSyncService = scope.ServiceProvider.GetRequiredService<IBidirectionalSyncService>();

                _logger.LogInformation("🔄 Manual sync to cloud requested - running full bidirectional sync instead.");
                var result = await bidirectionalSyncService.SyncAsync();

                if (!result.Success)
                {
                    var errorSummary = result.Errors != null && result.Errors.Any()
                        ? string.Join(", ", result.Errors)
                        : "Unknown error";

                    _logger.LogWarning($"⚠️ Manual bidirectional sync (ToCloud) completed with errors: {errorSummary}");
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in manual bidirectional sync (ToCloud)");
                return false;
            }
        }

        public async Task<bool> SyncFromCloudAsync()
        {
            try
            {
                using var scope = _services.CreateScope();
                var bidirectionalSyncService = scope.ServiceProvider.GetRequiredService<IBidirectionalSyncService>();

                _logger.LogInformation("🔄 Manual sync from cloud requested - running full bidirectional sync instead.");
                var result = await bidirectionalSyncService.SyncAsync();

                if (!result.Success)
                {
                    var errorSummary = result.Errors != null && result.Errors.Any()
                        ? string.Join(", ", result.Errors)
                        : "Unknown error";

                    _logger.LogWarning($"⚠️ Manual bidirectional sync (FromCloud) completed with errors: {errorSummary}");
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in manual bidirectional sync (FromCloud)");
                return false;
            }
        }

        public async Task<bool> FullSyncAsync()
        {
            try
            {
                using var scope = _services.CreateScope();
                var bidirectionalSyncService = scope.ServiceProvider.GetRequiredService<IBidirectionalSyncService>();

                _logger.LogInformation("🔄 Manual full sync requested - running full bidirectional sync.");
                var result = await bidirectionalSyncService.SyncAsync();

                if (!result.Success)
                {
                    var errorSummary = result.Errors != null && result.Errors.Any()
                        ? string.Join(", ", result.Errors)
                        : "Unknown error";

                    _logger.LogWarning($"⚠️ Manual bidirectional full sync completed with errors: {errorSummary}");
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in manual bidirectional full sync");
                return false;
            }
        }

        public void Dispose()
        {
            _syncTimer?.Stop();
            _syncTimer?.Dispose();
            _logger.LogInformation("Sync timer disposed");
        }
    }
}