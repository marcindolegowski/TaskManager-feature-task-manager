namespace Accessory.Builder.MessageBus.Common;

public class BusProperties
{
    public string? Host { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public ushort Port { get; set; }
    public string? VirtualHost { get; set; }
    public string? EventExchangeName { get; set; }
    public string? EventQueueNameName { get; set; }
}