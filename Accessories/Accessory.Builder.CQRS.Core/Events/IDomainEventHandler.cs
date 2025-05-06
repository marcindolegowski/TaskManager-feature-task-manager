using System.Threading.Tasks;
using Accessory.Builder.Core.Domain;

namespace Accessory.Builder.CQRS.Core.Events;

public interface IDomainEventHandler<TEvent> where TEvent : class, IDomainEvent
{
    Task Handle(TEvent @event);
}