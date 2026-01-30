namespace ControlHub.SharedKernel.Common
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; }
        public int PageIndex { get; }
        public int PageSize { get; }
        public long TotalCount { get; }
        public int TotalPages { get; }
        public bool HasNextPage => PageIndex * PageSize < TotalCount;
        public bool HasPreviousPage => PageIndex > 1;

        public PaginatedResult(List<T> items, long count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

        public static PaginatedResult<T> Create(List<T> items, long count, int pageIndex, int pageSize)
        {
            return new PaginatedResult<T>(items, count, pageIndex, pageSize);
        }
    }
}
