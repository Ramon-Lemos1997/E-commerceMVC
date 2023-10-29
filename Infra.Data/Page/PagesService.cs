using X.PagedList;
using Domain.Interfaces.Infra.Data;

namespace Infra.Data.Pagination
{
    public class PagesService : IPagesInterface
    {
        public async Task<IPagedList<T>> PaginationProductsAsync<T>(IEnumerable<T> data, int? page)
        {
            int pageNumber = page ?? 1; 
            const int pageSize = 30;

            var pagedList = data.ToPagedList(pageNumber, pageSize);

            return pagedList;
        }
    }
}
