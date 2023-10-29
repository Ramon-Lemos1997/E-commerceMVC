using X.PagedList;

namespace Domain.Interfaces.Infra.Data
{
    public interface IPagesInterface
    {
        Task<IPagedList<T>> PaginationProductsAsync<T>(IEnumerable<T> data, int? page);
    }
}
