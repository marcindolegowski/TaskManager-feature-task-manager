using System.Threading.Tasks;
using Accessory.Builder.Core.Domain;

namespace Accessory.Builder.CQRS.Core.Events;

public interface IDomainEventDispatcher
{
    Task Send<T>(T @event) where T : class, IDomainEvent;
}