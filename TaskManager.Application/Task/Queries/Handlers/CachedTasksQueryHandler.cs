using Accessory.Builder.CQRS.Core.Queries;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries.Handlers;

public class CachedTasksQueryHandler : IQueryHandler<TasksQuery, IEnumerable<TaskDTO>>
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions
    {
        AbsoluteExpiration = new DateTimeOffset(new DateTime(2089, 10, 17, 07, 00, 00), TimeSpan.Zero),
        SlidingExpiration = TimeSpan.FromSeconds(3600)
    };
    private readonly ILogger<CachedTasksQueryHandler> _logger;

    public CachedTasksQueryHandler(
        IDistributedCache distributedCache,
        IQueryDispatcher queryDispatcher,
        ILogger<CachedTasksQueryHandler> logger)
    {
        _distributedCache = distributedCache;
        _queryDispatcher = queryDispatcher;
        _logger = logger;
    }

    private const string Task = "50d858eaae3de4a3a1794ec9f40c0d63b4cc4e9335151f770afe7a9dbf1fbe7c";

    public async Task<IEnumerable<TaskDTO>> HandleAsync(TasksQuery query)
    {
        try
        {
            var cacheItem = await _distributedCache.GetAsync(Task);
            if (cacheItem != null)
            {
                var cacheItemAsString = Encoding.UTF8.GetString(cacheItem);
                var result = JsonConvert.DeserializeObject<List<TaskDTO>>(cacheItemAsString);
                return result!;
            }
            var dtos = await _queryDispatcher.QueryAsync(new TasksQuery());
            if (dtos != null && dtos.Count() > 0)
                await StoreInCache(dtos);
            return dtos!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to establish connection with cache");
            return null!;
        }
    }

    public async System.Threading.Tasks.Task StoreInCache(IEnumerable<TaskDTO> entities)
    {
        await _distributedCache.SetAsync(Task, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entities)),
            _cacheOptions);
    }
}