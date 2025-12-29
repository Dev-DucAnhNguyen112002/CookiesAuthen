using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace CookiesAuthen.Application.Common.Interfaces.Repository;
public interface IRepository<T> where T : class
{
    Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        IMapper? mapper = null, int pageIndex = 1,
        int pageSize = 20, int rowModify = 0, bool disableTracking = true);

    Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(IQueryable<T> query,
    IMapper? mapper = null, int pageIndex = 1,
    int pageSize = 20, int rowModify = 0, bool disableTracking = true);
}
