using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using SmartEduERP.Data;
using SmartEduERP.Services;
using System.Reflection;

namespace SmartEduERP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("SmartEduERP.appsettings.json");
            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
                builder.Configuration.AddConfiguration(config);
            }

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
            builder.Services.AddAuthorizationCore(options =>
            {
                options.FallbackPolicy = null;
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            });

            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            builder.Services.AddScoped(provider =>
                (AuthStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
            builder.Services.AddDbContextFactory<SmartEduDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });
            // Provide SmartEduDbContext via DI using the factory
            builder.Services.AddScoped<SmartEduDbContext>(sp =>
                sp.GetRequiredService<IDbContextFactory<SmartEduDbContext>>().CreateDbContext());
            builder.Services.AddDbContextFactory<AccountingDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });
            // Provide AccountingDbContext via DI using the factory
            builder.Services.AddScoped<AccountingDbContext>(sp =>
                sp.GetRequiredService<IDbContextFactory<AccountingDbContext>>().CreateDbContext());

            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddSingleton<SyncTimerService>();
            builder.Services.AddScoped<IDatabaseSyncService, DatabaseSyncService>();
            builder.Services.AddScoped<IBidirectionalSyncService, BidirectionalSyncService>();
            builder.Services.AddScoped<IToastService, ToastService>();
            builder.Services.AddScoped<ISyncQueueService, SyncQueueService>();
            builder.Services.AddScoped<StudentService>();
            builder.Services.AddScoped<TeacherService>();
            builder.Services.AddScoped<SubjectService>();
            builder.Services.AddScoped<EnrollmentService>();
            builder.Services.AddScoped<GradeService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<AccountingService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ExportService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<FileUploadService>();
            builder.Services.AddScoped<ActivityService>();
            builder.Services.AddScoped<SubmissionService>();
            builder.Services.AddScoped<AnnouncementService>();
            builder.Services.AddScoped<AttendanceService>();
            builder.Services.AddScoped<HrService>();
            builder.Services.AddScoped<AuditLogService>();
            builder.Services.AddScoped<IGlobalSyncService, GlobalSyncService>();

            var app = builder.Build();

            _ = Task.Run(async () =>
            {

                await Task.Delay(5000);
                var syncTimer = app.Services.GetRequiredService<SyncTimerService>();

            });

            return app;
        }
    }
}