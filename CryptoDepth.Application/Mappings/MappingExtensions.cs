using CryptoDepth.Application.Paggination;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoDepth.Application.Mappings
{
    public static class MappingExtensions
    {
        public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
            => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);
        public static PaginatedList<TDestination> PaginatedListResult<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
           => PaginatedList<TDestination>.CreateDuplicate(queryable.AsNoTracking(), pageNumber, pageSize);
    }
}