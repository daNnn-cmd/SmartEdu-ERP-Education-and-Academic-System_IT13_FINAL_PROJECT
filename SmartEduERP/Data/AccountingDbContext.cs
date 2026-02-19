using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Data;

public class AccountingDbContext : DbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeacherIncome> TeacherIncomes { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<Allowance> Allowances { get; set; }
    public DbSet<AccountingEntry> AccountingEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TeacherIncome>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tax>().HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        modelBuilder.Entity<Allowance>().HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        modelBuilder.Entity<AccountingEntry>().HasQueryFilter(e => !e.IsDeleted);
    }
}
