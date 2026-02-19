using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;
using System.Text.Json;

namespace SmartEduERP.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly AuditLogService _auditLogService;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public AuthStateProvider(IJSRuntime jsRuntime, AuditLogService auditLogService)
    {
        _jsRuntime = jsRuntime;
        _auditLogService = auditLogService;
        _ = InitializeAuthStateAsync();
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await InitializeAuthStateAsync();
        return new AuthenticationState(_currentUser);
    }

    private async Task InitializeAuthStateAsync()
    {
        try
        {
            // Try to load auth state from localStorage
            var authDataJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authData");

            if (!string.IsNullOrEmpty(authDataJson))
            {
                var authData = JsonSerializer.Deserialize<AuthData>(authDataJson);
                if (authData != null && !string.IsNullOrEmpty(authData.Username))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, authData.UserId.ToString()),
                        new Claim(ClaimTypes.Name, authData.Username),
                        new Claim(ClaimTypes.Role, authData.Role),
                        new Claim(ClaimTypes.GivenName, authData.FirstName ?? ""),
                        new Claim(ClaimTypes.Surname, authData.LastName ?? "")
                    };

                    var identity = new ClaimsIdentity(claims, "SmartEduERP Authentication");
                    _currentUser = new ClaimsPrincipal(identity);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing auth state: {ex.Message}");
        }

        // Default to unauthenticated
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    }

    public async void MarkUserAsAuthenticated(int userId, string username, string role, string firstName, string lastName)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName)
            };

            var identity = new ClaimsIdentity(claims, "SmartEduERP Authentication");
            _currentUser = new ClaimsPrincipal(identity);

            // Save to localStorage
            var authData = new AuthData
            {
                UserId = userId,
                Username = username,
                Role = role,
                FirstName = firstName,
                LastName = lastName
            };

            var authDataJson = JsonSerializer.Serialize(authData);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authData", authDataJson);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking user as authenticated: {ex.Message}");
        }
    }

    public async void MarkUserAsLoggedOut()
    {
        try
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            // Clear localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authData");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking user as logged out: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        // Get current user info before logging out
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Log logout action
        if (userId.HasValue && !string.IsNullOrEmpty(username))
        {
            try
            {
                await _auditLogService.LogActionAsync(
                    action: "Logout",
                    tableName: "UserAccounts",
                    recordId: userId.Value,
                    userId: userId.Value,
                    oldValues: null,
                    newValues: $"User '{username}' logged out"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging audit: {ex.Message}");
            }
        }

        // Mark user as logged out
        MarkUserAsLoggedOut();
    }

    public string? GetCurrentUsername()
    {
        return _currentUser.Identity?.Name;
    }

    public string? GetCurrentUserRole()
    {
        return _currentUser.FindFirst(ClaimTypes.Role)?.Value;
    }

    public int? GetCurrentUserId()
    {
        var userIdClaim = _currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public string? GetCurrentUserIdString()
    {
        return _currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public bool IsAuthenticated()
    {
        return _currentUser.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        return _currentUser.IsInRole(role);
    }

    // Helper class for storing auth data
    private class AuthData
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}