using System.Threading.Tasks;

namespace Accessory.Builder.CQRS.Core.Commands;

public interface ICommandHandler<in TCommand> where TCommand : class, ICommand 
{
    Task Handle(TCommand command);
}