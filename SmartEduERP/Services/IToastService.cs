using Microsoft.AspNetCore.Components;

namespace SmartEduERP.Services
{
    public interface IToastService
    {
        event Action<string, string, string> OnShow;
        void ShowSuccess(string message, string title = "Success");
        void ShowError(string message, string title = "Error");
        void ShowWarning(string message, string title = "Warning");
        void ShowInfo(string message, string title = "Information");

        // Add async versions
        Task ShowSuccessAsync(string message, string title = "Success");
        Task ShowErrorAsync(string message, string title = "Error");
        Task ShowWarningAsync(string message, string title = "Warning");
        Task ShowInfoAsync(string message, string title = "Information");
    }

    public class ToastService : IToastService
    {
        public event Action<string, string, string>? OnShow;

        public void ShowSuccess(string message, string title = "Success")
        {
            OnShow?.Invoke(message, title, "success");
        }

        public void ShowError(string message, string title = "Error")
        {
            OnShow?.Invoke(message, title, "error");
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            OnShow?.Invoke(message, title, "warning");
        }

        public void ShowInfo(string message, string title = "Information")
        {
            OnShow?.Invoke(message, title, "info");
        }

        // Async versions that wrap the synchronous calls
        public Task ShowSuccessAsync(string message, string title = "Success")
        {
            ShowSuccess(message, title);
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, string title = "Error")
        {
            ShowError(message, title);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string message, string title = "Warning")
        {
            ShowWarning(message, title);
            return Task.CompletedTask;
        }

        public Task ShowInfoAsync(string message, string title = "Information")
        {
            ShowInfo(message, title);
            return Task.CompletedTask;
        }
    }
}