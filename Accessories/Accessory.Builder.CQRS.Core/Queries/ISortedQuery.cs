using System.Collections.Generic;
using Accessory.Builder.CQRS.Core.Queries.DTO;

namespace Accessory.Builder.CQRS.Core.Queries;

public interface ISortedQuery<T> : ISortedQuery, IQuery<T>
{
}

public interface ISortedQuery : IQuery
{
    IEnumerable<SortingConfiguration>? SortingConfiguration { get; }
}