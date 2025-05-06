using Accessory.Builder.Core.Domain;

namespace Accessory.Builder.Outbox.Schedulers;

public interface IDomainEventScheduler
{
    Task Enqueue(IDomainEvent domainEvent);
}