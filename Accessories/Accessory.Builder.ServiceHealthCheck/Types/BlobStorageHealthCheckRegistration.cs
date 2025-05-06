using Accessory.Builder.Core.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.ServiceHealthCheck.Types;

public class BlobStorageServiceHealthCheck : IServiceHealthCheck
{
    private readonly string _serviceName;

    public BlobStorageServiceHealthCheck(string serviceName)
    {
        _serviceName = serviceName;
    }

    //TODO: Move to the specific BlobStorage Accessory dll
    private class BlobStorageProperties
    {
        public string? ConnectionString { get; set; }
    }
    
    public void Register(IAccessoryBuilder AccessoryBuilder, IHealthChecksBuilder healthChecksBuilder)
    {
        if (AccessoryBuilder is null) throw new ArgumentNullException($"{nameof(IAccessoryBuilder)}");

        var blobStorageProperties = AccessoryBuilder.GetSettings<BlobStorageProperties>(_serviceName);

        if (blobStorageProperties?.ConnectionString is null)
            throw new ArgumentException($"{nameof(BlobStorageProperties)} could not be loaded from configuration. Please check, if section names are matching");

        healthChecksBuilder.AddAzureBlobStorage(
            blobStorageProperties.ConnectionString,
            name: "BlobStorage",
            tags: new[] { "Azure", "BlobStorage" }
        );
    }
}