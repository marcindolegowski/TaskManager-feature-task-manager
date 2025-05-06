using System;
using Accessory.Builder.Core.Common;

namespace Accessory.Builder.Core.Domain;

public abstract class DomainEventBase : IDomainEvent
{
    public DateTimeOffset OccuredOn { get; } = SystemTime.OffsetNow();
}