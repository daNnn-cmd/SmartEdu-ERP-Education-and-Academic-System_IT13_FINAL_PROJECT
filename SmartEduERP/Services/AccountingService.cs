using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class AccountingService
{
    private readonly AccountingDbContext _context;

    public AccountingService(AccountingDbContext context)
    {
        _context = context;
    }

    // Teacher income
    public async Task<List<TeacherIncome>> GetTeacherIncomesAsync(DateTime? start = null, DateTime? end = null)
    {
        var query = _context.TeacherIncomes
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (start.HasValue)
        {
            query = query.Where(t => t.PeriodStartDate >= start.Value);
        }

        if (end.HasValue)
        {
            query = query.Where(t => t.PeriodEndDate <= end.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.TeacherIncomeId)
            .ToListAsync();
    }

    public async Task<TeacherIncome> CreateTeacherIncomeAsync(TeacherIncome income)
    {
        ValidationHelper.SanitizeAndValidateModel(income);

        var now = DateTime.UtcNow;
        income.CreatedAt = now;
        income.UpdatedAt = now;

        _context.TeacherIncomes.Add(income);
        await _context.SaveChangesAsync();
        return income;
    }

    public async Task<TeacherIncome?> UpdateTeacherIncomeAsync(int id, TeacherIncome updated)
    {
        var income = await _context.TeacherIncomes.FirstOrDefaultAsync(t => t.TeacherIncomeId == id && !t.IsDeleted);
        if (income == null)
        {
            return null;
        }

        income.TeacherId = updated.TeacherId;
        income.TeacherName = updated.TeacherName;
        income.PeriodStartDate = updated.PeriodStartDate;
        income.PeriodEndDate = updated.PeriodEndDate;
        income.BasicSalary = updated.BasicSalary;
        income.OvertimePay = updated.OvertimePay;
        income.OtherIncome = updated.OtherIncome;
        income.UpdatedAt = DateTime.UtcNow;

        ValidationHelper.SanitizeAndValidateModel(income);
        await _context.SaveChangesAsync();
        return income;
    }

    public async Task<bool> SoftDeleteTeacherIncomeAsync(int id)
    {
        var income = await _context.TeacherIncomes.FirstOrDefaultAsync(t => t.TeacherIncomeId == id);
        if (income == null)
            return false;

        income.IsDeleted = true;
        income.DeletedAt = DateTime.UtcNow;
        income.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Taxes
    public async Task<List<Tax>> GetTaxesAsync()
    {
        return await _context.Taxes
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.TaxId)
            .ToListAsync();
    }

    public async Task<Tax> CreateTaxAsync(Tax tax)
    {
        ValidationHelper.SanitizeAndValidateModel(tax);

        var now = DateTime.UtcNow;
        tax.CreatedAt = now;
        tax.UpdatedAt = now;
        tax.IsDeleted = false;

        _context.Taxes.Add(tax);
        await _context.SaveChangesAsync();
        return tax;
    }

    public async Task<Tax?> UpdateTaxAsync(int id, Tax updated)
    {
        var tax = await _context.Taxes.FirstOrDefaultAsync(t => t.TaxId == id && !t.IsDeleted);
        if (tax == null)
        {
            return null;
        }

        tax.Name = updated.Name;
        tax.Description = updated.Description;
        tax.Rate = updated.Rate;
        tax.IsPercentage = updated.IsPercentage;
        tax.UpdatedAt = DateTime.UtcNow;

        ValidationHelper.SanitizeAndValidateModel(tax);
        await _context.SaveChangesAsync();
        return tax;
    }

    public async Task<bool> SoftDeleteTaxAsync(int id)
    {
        var tax = await _context.Taxes.FirstOrDefaultAsync(t => t.TaxId == id);
        if (tax == null)
            return false;

        tax.IsDeleted = true;
        tax.DeletedAt = DateTime.UtcNow;
        tax.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Allowances
    public async Task<List<Allowance>> GetAllowancesAsync()
    {
        return await _context.Allowances
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.AllowanceId)
            .ToListAsync();
    }

    public async Task<Allowance> CreateAllowanceAsync(Allowance allowance)
    {
        ValidationHelper.SanitizeAndValidateModel(allowance);

        var now = DateTime.UtcNow;
        allowance.CreatedAt = now;
        allowance.UpdatedAt = now;
        allowance.IsDeleted = false;

        _context.Allowances.Add(allowance);
        await _context.SaveChangesAsync();
        return allowance;
    }

    public async Task<Allowance?> UpdateAllowanceAsync(int id, Allowance updated)
    {
        var allowance = await _context.Allowances.FirstOrDefaultAsync(a => a.AllowanceId == id && !a.IsDeleted);
        if (allowance == null)
        {
            return null;
        }

        allowance.Name = updated.Name;
        allowance.Description = updated.Description;
        allowance.Amount = updated.Amount;
        allowance.IsPercentage = updated.IsPercentage;
        allowance.IsTaxable = updated.IsTaxable;
        allowance.UpdatedAt = DateTime.UtcNow;

        ValidationHelper.SanitizeAndValidateModel(allowance);
        await _context.SaveChangesAsync();
        return allowance;
    }

    public async Task<bool> SoftDeleteAllowanceAsync(int id)
    {
        var allowance = await _context.Allowances.FirstOrDefaultAsync(a => a.AllowanceId == id);
        if (allowance == null)
            return false;

        allowance.IsDeleted = true;
        allowance.DeletedAt = DateTime.UtcNow;
        allowance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Simple net income calculation using current active taxes and allowances
    public (decimal GrossIncome, decimal TotalAllowances, decimal TaxableBase, decimal TotalTax, decimal NetIncome) CalculateNetIncome(
        TeacherIncome income,
        IEnumerable<Allowance> allowances,
        IEnumerable<Tax> taxes)
    {
        var baseIncome = income.BasicSalary + income.OvertimePay + income.OtherIncome;

        decimal totalAllowances = 0m;
        decimal taxableAllowances = 0m;

        foreach (var allowance in allowances.Where(a => a.IsActive && !a.IsDeleted))
        {
            decimal value;
            if (allowance.IsPercentage)
            {
                value = Math.Round(baseIncome * (allowance.Amount / 100m), 2);
            }
            else
            {
                value = allowance.Amount;
            }

            totalAllowances += value;
            if (allowance.IsTaxable)
            {
                taxableAllowances += value;
            }
        }

        var taxableBase = baseIncome + taxableAllowances;

        decimal totalTax = 0m;
        foreach (var tax in taxes.Where(t => t.IsActive && !t.IsDeleted))
        {
            decimal value;
            if (tax.IsPercentage)
            {
                value = Math.Round(taxableBase * (tax.Rate / 100m), 2);
            }
            else
            {
                value = tax.Rate;
            }

            totalTax += value;
        }

        var netIncome = baseIncome + totalAllowances - totalTax;
        return (baseIncome, totalAllowances, taxableBase, totalTax, netIncome);
    }
}
