using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CryptoDepth.Application.Paggination
{
    public static class PaginationService
    {

        public static async Task<Pagination<T>> GetPagination<T>(List<T> query, int page, int pageSize) where T : class
        {
            if (page == 0 || pageSize == 0)
            {
                Pagination<T> paginationFull = new Pagination<T>
                {
                    TotalItems = query.Count(),
                    PageSize = pageSize,
                    CurrentPage = page,
                };
                paginationFull.Result = query.ToList();
                paginationFull.TotalPages = 1;
                return paginationFull;
            }
            Pagination<T> pagination = new Pagination<T>
            {
                TotalItems = query.Count(),
                PageSize = pageSize,
                CurrentPage = page,
            };

            int skip = (page - 1) * pageSize;
            var props = typeof(T).GetProperties();
            PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(T)).Find("Id", true);
            var orderByProperty = props.FirstOrDefault(n => n.Name == "Id");

            if (orderByProperty == null)
            {
                throw new Exception($"Field: 'Id' is not sortable");
            }

            pagination.Result = query
                //.OrderBy(i => orderByProperty.GetValue(i, null))
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            pagination.TotalPages = (pagination.TotalItems + pagination.PageSize - 1) / pagination.PageSize;
            return pagination;
        }
    }

    public class Pagination<T>
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }

        public string OrderBy { get; set; }

        public bool OrderByDesc { get; set; }

        public List<T> Result { get; set; }
    }
}
