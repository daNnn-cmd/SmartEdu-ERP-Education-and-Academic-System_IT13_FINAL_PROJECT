using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Timers;

namespace SmartEduERP.Services;

public class ConnectivityChangedEventArgs : EventArgs
{
    public bool IsWifiConnected { get; init; }
    public bool HasInternet { get; init; }
    public string? NetworkType { get; init; }
}

public interface IConnectivityService : IDisposable
{
    event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

    bool IsWifiConnected { get; }
    bool HasInternet { get; }

    void StartMonitoring();
    void StopMonitoring();
}

public class ConnectivityService : IConnectivityService
{
    private readonly ILogger<ConnectivityService> _logger;
    private readonly IConfiguration _configuration;
    private System.Timers.Timer? _timer;
    private bool _isWifiConnected;
    private bool _hasInternet;

    public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

    public bool IsWifiConnected => _isWifiConnected;
    public bool HasInternet => _hasInternet;

    public ConnectivityService(ILogger<ConnectivityService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        var intervalSeconds = _configuration.GetValue<int>("DatabaseSync:ConnectivityCheckIntervalSeconds", 10);
        _timer = new System.Timers.Timer(TimeSpan.FromSeconds(intervalSeconds).TotalMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void StartMonitoring()
    {
        if (_timer == null)
        {
            InitializeTimer();
        }

        _timer!.Start();

        _ = CheckConnectivityAsync(true);
    }

    public void StopMonitoring()
    {
        _timer?.Stop();
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await CheckConnectivityAsync();
    }

    private async Task CheckConnectivityAsync(bool forceRaise = false)
    {
        bool wifiConnected = false;
        bool hasInternet = false;

        try
        {
            wifiConnected = NetworkInterface.GetIsNetworkAvailable() &&
                            NetworkInterface.GetAllNetworkInterfaces().Any(nic =>
                                nic.OperationalStatus == OperationalStatus.Up &&
                                (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                                 nic.Description.Contains("wi-fi", StringComparison.OrdinalIgnoreCase) ||
                                 nic.Name.Contains("wi-fi", StringComparison.OrdinalIgnoreCase)));

            if (wifiConnected)
            {
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync("8.8.8.8", 3000);
                    hasInternet = reply.Status == IPStatus.Success;
                }
                catch (Exception pingEx)
                {
                    _logger.LogWarning(pingEx, "Error while pinging for internet connectivity");
                    hasInternet = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking network connectivity");
        }

        if (!forceRaise && wifiConnected == _isWifiConnected && hasInternet == _hasInternet)
        {
            return;
        }

        _isWifiConnected = wifiConnected;
        _hasInternet = hasInternet;

        var args = new ConnectivityChangedEventArgs
        {
            IsWifiConnected = wifiConnected,
            HasInternet = hasInternet,
            NetworkType = wifiConnected ? "WiFi" : "None"
        };

        ConnectivityChanged?.Invoke(this, args);

        _logger.LogInformation($"Connectivity changed - WiFi: {wifiConnected}, Internet: {hasInternet}");
    }

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
