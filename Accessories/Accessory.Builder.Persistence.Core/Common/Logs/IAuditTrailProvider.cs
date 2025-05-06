using System.Threading.Tasks;

namespace Accessory.Builder.Persistence.Core.Common.Logs;

public interface IAuditTrailProvider
{
    Task LogChangesAsync();
}