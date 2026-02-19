using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class PaymentService
{
    private readonly SmartEduDbContext _context;
    private readonly AuditLogService _auditLogService;
    private readonly ISyncQueueService _syncQueueService;

    public PaymentService(SmartEduDbContext context, AuditLogService auditLogService, ISyncQueueService syncQueueService)
    {
        _context = context;
        _auditLogService = auditLogService;
        _syncQueueService = syncQueueService;
    }

    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Student)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.PaymentId == id && !p.IsDeleted);
    }

    // Add this method for getting payment with details
    public async Task<Payment?> GetPaymentWithDetailsAsync(int id)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Student)
            .Where(p => p.PaymentId == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    // NEW METHOD: Get payment with audit logging for viewing details
    public async Task<Payment?> GetPaymentWithAuditLogAsync(int id, int? userId = null)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.PaymentId == id && !p.IsDeleted);

        if (payment != null)
        {
            // Log the view action
            try
            {
                var paymentJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    payment.PaymentId,
                    payment.StudentId,
                    payment.Amount,
                    payment.PaymentDate,
                    payment.PaymentMethod,
                    payment.PaymentStatus,
                    StudentName = payment.Student != null ? $"{payment.Student.FirstName} {payment.Student.LastName}" : "Unknown"
                });

                await _auditLogService.LogActionAsync(
                    action: "View",
                    tableName: "Payments",
                    recordId: payment.PaymentId,
                    userId: userId,
                    oldValues: null,
                    newValues: paymentJson
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging payment view audit: {ex.Message}");
            }
        }

        return payment;
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment, int? userId = null)
    {
        // Sanitize and validate payment before saving or syncing
        ValidationHelper.SanitizeAndValidateModel(payment);

        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Log payment creation
        try
        {
            var newPaymentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                payment.PaymentId,
                payment.StudentId,
                payment.Amount,
                payment.PaymentDate,
                payment.PaymentMethod,
                payment.PaymentStatus
            });

            await _auditLogService.LogActionAsync(
                action: "Create",
                tableName: "Payments",
                recordId: payment.PaymentId,
                userId: userId,
                oldValues: null,
                newValues: newPaymentJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Create",
                "Payments",
                payment.PaymentId,
                new
                {
                    payment.PaymentId,
                    payment.StudentId,
                    payment.Amount,
                    payment.PaymentDate,
                    payment.PaymentMethod,
                    payment.PaymentStatus
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing payment create sync: {ex.Message}");
        }

        return payment;
    }

    public async Task<Payment?> UpdatePaymentAsync(Payment payment, int? userId = null)
    {
        // Sanitize and validate incoming payment data before applying updates
        ValidationHelper.SanitizeAndValidateModel(payment);

        var existing = await _context.Payments.FindAsync(payment.PaymentId);
        if (existing == null)
            return null;

        // Capture old values
        var oldPaymentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            existing.PaymentId,
            existing.StudentId,
            existing.Amount,
            existing.PaymentStatus
        });

        existing.StudentId = payment.StudentId;
        existing.Amount = payment.Amount;
        existing.PaymentDate = payment.PaymentDate;
        existing.PaymentMethod = payment.PaymentMethod;
        existing.PaymentStatus = payment.PaymentStatus;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log payment update
        try
        {
            var newPaymentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                existing.PaymentId,
                existing.StudentId,
                existing.Amount,
                existing.PaymentStatus
            });

            await _auditLogService.LogActionAsync(
                action: "Update",
                tableName: "Payments",
                recordId: payment.PaymentId,
                userId: userId,
                oldValues: oldPaymentJson,
                newValues: newPaymentJson
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Update",
                "Payments",
                existing.PaymentId,
                new
                {
                    existing.PaymentId,
                    existing.StudentId,
                    existing.Amount,
                    existing.PaymentStatus
                },
                new
                {
                    existing.PaymentId,
                    existing.StudentId,
                    existing.Amount,
                    existing.PaymentStatus
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing payment update sync: {ex.Message}");
        }

        return existing;
    }

    public async Task<bool> SoftDeletePaymentAsync(int id, int? userId = null)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return false;

        var deletedPaymentJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            payment.PaymentId,
            payment.StudentId,
            payment.Amount,
            payment.PaymentDate,
            payment.PaymentStatus
        });

        payment.IsDeleted = true;
        payment.DeletedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        try
        {
            await _auditLogService.LogActionAsync(
                action: "Delete",
                tableName: "Payments",
                recordId: payment.PaymentId,
                userId: userId,
                oldValues: deletedPaymentJson,
                newValues: "Payment soft deleted"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging payment delete audit: {ex.Message}");
        }

        try
        {
            await _syncQueueService.QueueOperationAsync(
                "Delete",
                "Payments",
                payment.PaymentId,
                new
                {
                    payment.PaymentId,
                    payment.StudentId,
                    payment.Amount,
                    payment.PaymentDate
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing payment delete sync: {ex.Message}");
        }

        return true;
    }

    public async Task<List<Payment>> GetPaymentsByStudentAsync(int studentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.StudentId == studentId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByStatusAsync(string status)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Student)
            .Where(p => p.PaymentStatus == status && !p.IsDeleted)
            .ToListAsync();
    }

    // UPDATED: Added filter parameters
    public async Task<decimal> GetTotalPaymentAmountAsync(int? year = null, int? month = null)
    {
        var query = _context.Payments.Where(p => !p.IsDeleted);

        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                // Filter by specific year and month
                query = query.Where(p => p.PaymentDate.Year == year.Value &&
                                        p.PaymentDate.Month == month.Value);
            }
            else
            {
                // Filter by year only
                query = query.Where(p => p.PaymentDate.Year == year.Value);
            }
        }

        return await query.SumAsync(p => p.Amount);
    }

    public async Task<decimal> GetTotalPaymentsByStudentAsync(int studentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.StudentId == studentId && !p.IsDeleted)
            .SumAsync(p => p.Amount);
    }

    public async Task<List<Payment>> GetPaymentsByStudentIdAsync(int studentId)
    {
        return await GetPaymentsByStudentAsync(studentId);
    }

    public async Task<int> GetTotalPaymentCountAsync()
    {
        return await _context.Payments.CountAsync(p => !p.IsDeleted);
    }

    // Add this method to get payment statistics
    public async Task<PaymentStatistics> GetPaymentStatisticsAsync()
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return new PaymentStatistics
        {
            TotalPayments = payments.Count,
            TotalAmount = payments.Sum(p => p.Amount),
            PaidCount = payments.Count(p => p.PaymentStatus == "Paid"),
            PendingCount = payments.Count(p => p.PaymentStatus == "Pending"),
            FailedCount = payments.Count(p => p.PaymentStatus == "Failed")
        };
    }

    // ANALYTICS METHODS - UPDATED WITH FILTER PARAMETERS:

    // UPDATED: Added filter parameters and time range support
    public async Task<List<RevenueData>> GetRevenueDataAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var query = _context.Payments.Where(p => !p.IsDeleted);

        // Apply year/month filters
        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                query = query.Where(p => p.PaymentDate.Year == year.Value &&
                                        p.PaymentDate.Month == month.Value);
            }
            else
            {
                query = query.Where(p => p.PaymentDate.Year == year.Value);
            }
        }

        var payments = await query.ToListAsync();

        List<RevenueData> revenueData = timeRange.ToLower() switch
        {
            "year" => GetYearlyRevenueData(payments),
            "quarter" => GetQuarterlyRevenueData(payments),
            _ => GetMonthlyRevenueData(payments) // Default to monthly
        };

        return revenueData;
    }

    // UPDATED: Added filter parameters and time range support
    public async Task<decimal> GetRevenueGrowthPercentageAsync(int? year = null, int? month = null, string timeRange = "month")
    {
        var currentDate = DateTime.Now;
        DateTime startDate, endDate;
        DateTime previousStartDate, previousEndDate;

        // Calculate date ranges based on timeRange
        switch (timeRange.ToLower())
        {
            case "year":
                startDate = new DateTime(currentDate.Year, 1, 1);
                endDate = new DateTime(currentDate.Year, 12, 31);
                previousStartDate = new DateTime(currentDate.Year - 1, 1, 1);
                previousEndDate = new DateTime(currentDate.Year - 1, 12, 31);
                break;
            case "quarter":
                var currentQuarter = (currentDate.Month - 1) / 3 + 1;
                startDate = new DateTime(currentDate.Year, (currentQuarter - 1) * 3 + 1, 1);
                endDate = startDate.AddMonths(3).AddDays(-1);
                previousStartDate = startDate.AddYears(-1);
                previousEndDate = endDate.AddYears(-1);
                break;
            default: // monthly
                startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = startDate.AddDays(-1);
                break;
        }

        // Apply year/month filters if provided
        if (year.HasValue)
        {
            if (month.HasValue && month > 0)
            {
                startDate = new DateTime(year.Value, month.Value, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                previousStartDate = startDate.AddMonths(-1);
                previousEndDate = startDate.AddDays(-1);
            }
            else
            {
                startDate = new DateTime(year.Value, 1, 1);
                endDate = new DateTime(year.Value, 12, 31);
                previousStartDate = startDate.AddYears(-1);
                previousEndDate = endDate.AddYears(-1);
            }
        }

        var currentPeriodRevenue = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                       p.PaymentDate >= startDate &&
                       p.PaymentDate <= endDate)
            .SumAsync(p => p.Amount);

        var previousPeriodRevenue = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                       p.PaymentDate >= previousStartDate &&
                       p.PaymentDate <= previousEndDate)
            .SumAsync(p => p.Amount);

        if (previousPeriodRevenue == 0)
            return currentPeriodRevenue > 0 ? 100 : 0;

        return ((currentPeriodRevenue - previousPeriodRevenue) / previousPeriodRevenue) * 100;
    }

    public async Task<Dictionary<string, int>> GetPaymentMethodDistributionAsync()
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return payments
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, decimal>> GetRevenueByPaymentMethodAsync()
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return payments
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
    }

    // NEW: Helper methods for different time ranges
    private List<RevenueData> GetMonthlyRevenueData(List<Payment> payments)
    {
        var monthlyData = payments
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new RevenueData
            {
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(d => d.Label)
            .ToList();

        return monthlyData;
    }

    private List<RevenueData> GetQuarterlyRevenueData(List<Payment> payments)
    {
        var quarterlyData = payments
            .GroupBy(p => new
            {
                Year = p.PaymentDate.Year,
                Quarter = (p.PaymentDate.Month - 1) / 3 + 1
            })
            .Select(g => new RevenueData
            {
                Label = $"Q{g.Key.Quarter} {g.Key.Year}",
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(d => d.Label)
            .ToList();

        return quarterlyData;
    }

    private List<RevenueData> GetYearlyRevenueData(List<Payment> payments)
    {
        var yearlyData = payments
            .GroupBy(p => p.PaymentDate.Year)
            .Select(g => new RevenueData
            {
                Label = g.Key.ToString(),
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(d => d.Label)
            .ToList();

        return yearlyData;
    }
}

// NEW: Added RevenueData class for dashboard
public class RevenueData
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

// Keep existing classes
public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
}

public class MonthlyRevenue
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Revenue { get; set; }
    public int PaymentCount { get; set; }
}

public class PaymentTrend
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public int PaymentCount { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
}