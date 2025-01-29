namespace Shared.Core.API;

public class PaginatedResult<TEntity> where TEntity : class
{
    public int PageIndex { get; }
    public int PageSize { get; }
    public IEnumerable<TEntity> Data { get; }

    public PaginatedResult(int pageIndex, int pageSize, IEnumerable<TEntity> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Data = data;
    }
}