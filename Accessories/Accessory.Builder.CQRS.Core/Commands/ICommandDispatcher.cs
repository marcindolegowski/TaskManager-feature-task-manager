using System.Threading.Tasks;

namespace Accessory.Builder.CQRS.Core.Commands;

public interface ICommandDispatcher
{
    Task Send<T>(T command) where T : class, ICommand;
}