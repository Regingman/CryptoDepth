using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoDepth.Application.Paggination
{
    public class PaginatedList<T>
    {
        public int CurrentPage { get; }
        public int PageSize { get; set; }
        public int TotalPages { get; }
        public int TotalItems { get; }
        public List<T> Result { get; }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalItems = count;
            Result = items;
        }

        //public bool HasPreviousPage => CurrentPage > 1;

        //public bool HasNextPage => CurrentPage < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber == 0 && pageSize == 0)
            {
                return new PaginatedList<T>(await source.ToListAsync(), source.Count(), 1, source.Count());
            }
            else
            {
                var count = await source.CountAsync();
                var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return new PaginatedList<T>(items, count, pageNumber, pageSize);
            }
        }
        public static PaginatedList<T> Create(List<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber == 0 && pageSize == 0)
            {
                return new PaginatedList<T>(source.ToList(), source.Count(), 1, source.Count());
            }
            else
            {
                var count = source.Count();
                var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return new PaginatedList<T>(items, count, pageNumber, pageSize);
            }
        }
        public static PaginatedList<T> CreateDuplicate(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber == 0 && pageSize == 0)
            {
                return new PaginatedList<T>(source.ToList(), source.Count(), 1, source.Count());
            }
            else
            {
                var count = source.Count();
                var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return new PaginatedList<T>(items, count, pageNumber, pageSize);
            }
        }
    }
}