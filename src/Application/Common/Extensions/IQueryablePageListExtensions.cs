

using CookiesAuthen.Application.Common.Models;

namespace CookiesAuthen.Application.Common.Extentions;

public static class IQueryablePageListExtensions
{
    /// <summary>
    /// Converts the specified source to <see cref="IPagedList{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <param name="source">The source to paging.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
    /// </param>
    /// <param name="indexFrom">The start index value.</param>
    /// <returns>An instance of the inherited from <see cref="IPagedList{T}"/> interface.</returns>
    public static async Task<PaginatedList<T>> ToPagedListAsync<T>(this IQueryable<T> source,
        int pageIndex,
        int pageSize,
        int rowModify)
    {
        var count = await source.CountAsync();
        if (pageSize == -1)
        {
            var pagedList = new PaginatedList<T>(await source.ToListAsync(), pageIndex, pageSize, count);

            return pagedList;
        }
        else
        {
            if (pageSize == 0)
                pageSize = 10;
            var items = await source.Skip(((pageIndex - 1) * pageSize) + rowModify)
                .Take(pageSize).ToListAsync();

            var pagedList = new PaginatedList<T>(items, pageIndex, pageSize, count);

            return pagedList;
        }
    }

    /// <summary>
    /// Converts the specified source to <see cref="IPagedList{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <param name="source">The source to paging.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
    /// </param>
    /// <param name="indexFrom">The start index value.</param>
    /// <returns>An instance of the inherited from <see cref="IPagedList{T}"/> interface.</returns>
    public static async Task<PaginatedList<T>> ToPagedListAsync<T>(this IEnumerable<T> source,
        int pageIndex,
        int pageSize,
        int rowModify)
    {
        var count = source.Count();
        if (pageSize == -1)
        {
            var pagedList = new PaginatedList<T>(source.ToList(), count, pageIndex, pageSize);

            return pagedList;
        }
        else
        {
            if (pageSize == 0)
                pageSize = 10;
            var items = source.Skip(((pageIndex - 1) * pageSize) + rowModify)
                .Take(pageSize).ToList();

            var pagedList = new PaginatedList<T>(items, pageIndex, pageSize, count);
          
            return await Task.FromResult(pagedList);
        }
    }
}
