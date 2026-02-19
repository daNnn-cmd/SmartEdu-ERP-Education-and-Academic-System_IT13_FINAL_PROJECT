using Microsoft.EntityFrameworkCore;
using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public partial class AuditLogService
{
    public async Task<List<AuditLog>> GetLogsForRecordAsync(string tableName, int recordId)
    {
        var query = GetBaseAuditQuery()
            .Where(a => a.TableName == tableName && a.RecordId == recordId);

        return await query
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }
}
