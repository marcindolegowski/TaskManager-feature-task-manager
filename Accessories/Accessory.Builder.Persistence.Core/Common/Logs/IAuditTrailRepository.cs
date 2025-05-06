using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accessory.Builder.Persistence.Core.Common.Logs;

public interface IAuditTrailRepository
{
    public Task AddRangeAsync(List<AuditTrail> audits);
}