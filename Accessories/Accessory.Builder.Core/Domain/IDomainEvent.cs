using System;

namespace Accessory.Builder.Core.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccuredOn { get; }
}