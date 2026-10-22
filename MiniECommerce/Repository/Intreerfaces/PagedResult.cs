namespace MiniECommerce.Repository.Interfaces
{
    /// <summary>One page of data plus the paging metadata needed to render a pager.</summary>
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int TotalPages);
}
