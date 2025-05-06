using System;
using System.Text;
using System.Threading.Tasks;
using Accessory.Builder.CQRS.Core.Queries;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries.Handlers;

public class SpecificCachedTaskQueryHandler : IQueryHandler<SpecificCachedTaskQuery, TaskDTO>
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions
    {
        AbsoluteExpiration = new DateTimeOffset(new DateTime(2089, 10, 17, 07, 00, 00), TimeSpan.Zero),
        SlidingExpiration = TimeSpan.FromSeconds(3600)
    };
    private readonly ILogger<SpecificCachedTaskQueryHandler> _logger;

    public SpecificCachedTaskQueryHandler(
        IDistributedCache distributedCache,
        IQueryDispatcher queryDispatcher,
        ILogger<SpecificCachedTaskQueryHandler> logger)
    {
        _distributedCache = distributedCache;
        _queryDispatcher = queryDispatcher;
        _logger = logger;
    }

    public async Task<TaskDTO> HandleAsync(SpecificCachedTaskQuery query)
    {
        try
        {
            var cacheItem = await _distributedCache.GetAsync(query.Name);
            if (cacheItem != null)
            {
                var cacheItemAsString = Encoding.UTF8.GetString(cacheItem);
                var result = JsonConvert.DeserializeObject<TaskDTO>(cacheItemAsString);
                return result!;
            }
            var dto = await _queryDispatcher.QueryAsync(new SpecificTaskQuery { Name = query.Name });
            if (dto != null && dto.Name != null)
                await StoreInCache(dto);
            return dto!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to establish connection with cache");
            return null!;
        }
    }

    public async System.Threading.Tasks.Task StoreInCache(TaskDTO entity)
    {
        string key = entity.Name ?? throw new ArgumentNullException(nameof(entity.Name));
        await _distributedCache.SetAsync(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity)),
            _cacheOptions);
    }
}