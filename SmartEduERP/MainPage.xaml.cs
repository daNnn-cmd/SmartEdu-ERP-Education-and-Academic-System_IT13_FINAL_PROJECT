using SmartEduERP.Services;

namespace SmartEduERP
{
    public partial class MainPage : ContentPage
    {
        private readonly SyncTimerService _syncService;

        public MainPage(SyncTimerService syncService)
        {
            InitializeComponent();
            _syncService = syncService;

            // You can add manual sync triggers here if needed
            InitializeSync();
        }

        private void InitializeSync()
        {
            // The sync service automatically starts via the timer
            // You can add manual sync buttons to your UI if desired
        }

        // Example methods you can call from your UI for manual sync
        public async Task ManualSyncToCloud()
        {
            try
            {
                await _syncService.SyncToCloudAsync();
                // Show success message
            }
            catch (Exception ex)
            {
                // Show error message
                System.Diagnostics.Debug.WriteLine($"Manual sync error: {ex.Message}");
            }
        }

        public async Task ManualSyncFromCloud()
        {
            try
            {
                await _syncService.SyncFromCloudAsync();
                // Show success message
            }
            catch (Exception ex)
            {
                // Show error message
                System.Diagnostics.Debug.WriteLine($"Manual sync error: {ex.Message}");
            }
        }

        public async Task ManualFullSync()
        {
            try
            {
                await _syncService.FullSyncAsync();
                // Show success message
            }
            catch (Exception ex)
            {
                // Show error message
                System.Diagnostics.Debug.WriteLine($"Manual sync error: {ex.Message}");
            }
        }
    }
}