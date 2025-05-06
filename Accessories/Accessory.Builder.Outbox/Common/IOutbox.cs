namespace Accessory.Builder.Outbox.Common;

public interface IOutbox
{
    void Add(OutboxMessage outbox);
}