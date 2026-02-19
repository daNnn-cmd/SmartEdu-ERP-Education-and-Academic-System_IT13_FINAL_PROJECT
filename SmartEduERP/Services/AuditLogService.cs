using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services
{
    public partial class AuditLogService
    {
        private readonly SmartEduDbContext _context;

        public AuditLogService(SmartEduDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string action, string tableName, int recordId, int? userId, string? oldValues = null, string? newValues = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                UserId = userId,
                OldValues = oldValues,
                NewValues = newValues,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        private IQueryable<AuditLog> GetBaseAuditQuery()
        {
            return _context.AuditLogs
                .Include(a => a.User)
                .Select(a => new AuditLog
                {
                    AuditId = a.AuditId,
                    UserId = a.UserId,
                    Action = a.Action,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    CreatedAt = a.CreatedAt,
                    User = a.User, // Keep the navigation property
                    Username = a.User != null ? a.User.Username : "System" // Populate the username
                });
        }

        public async Task<List<AuditLog>> GetAllLogsAsync()
        {
            return await GetBaseAuditQuery()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsAsync(int skip, int take)
        {
            return await GetBaseAuditQuery()
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByTableNameAsync(string tableName)
        {
            return await GetBaseAuditQuery()
                .Where(a => a.TableName == tableName)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByUserIdAsync(int userId)
        {
            return await GetBaseAuditQuery()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetBaseAuditQuery()
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }

        public async Task<List<AuditLog>> SearchLogsAsync(string searchTerm)
        {
            var lowerSearch = searchTerm.ToLower();
            return await GetBaseAuditQuery()
                .Where(a =>
                    a.Action.ToLower().Contains(lowerSearch) ||
                    (a.TableName != null && a.TableName.ToLower().Contains(lowerSearch)) ||
                    (a.OldValues != null && a.OldValues.ToLower().Contains(lowerSearch)) ||
                    (a.NewValues != null && a.NewValues.ToLower().Contains(lowerSearch)) ||
                    (a.Username != null && a.Username.ToLower().Contains(lowerSearch)))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByActionAsync(string action)
        {
            return await GetBaseAuditQuery()
                .Where(a => a.Action.ToLower() == action.ToLower())
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuthenticationLogsAsync()
        {
            var authActions = new[] { "login", "logout", "register" };
            return await GetBaseAuditQuery()
                .Where(a => authActions.Contains(a.Action.ToLower()))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetCrudLogsAsync()
        {
            var crudActions = new[] { "create", "update", "delete" };
            return await GetBaseAuditQuery()
                .Where(a => crudActions.Contains(a.Action.ToLower()))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByUserRoleAsync(string role)
        {
            return await GetBaseAuditQuery()
                .Where(a => a.User != null && a.User.Role != null && a.User.Role.ToLower() == role.ToLower())
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsWithFiltersAsync(int skip, int take, string? action = null, string? tableName = null, int? userId = null)
        {
            var query = GetBaseAuditQuery();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action.ToLower() == action.ToLower());

            if (!string.IsNullOrEmpty(tableName))
                query = query.Where(a => a.TableName == tableName);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}