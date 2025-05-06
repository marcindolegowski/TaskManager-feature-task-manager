using System.Threading.Tasks;

namespace Accessory.Builder.Core.Initializer;

public interface IInitializer
{
    Task InitializeAsync();
}