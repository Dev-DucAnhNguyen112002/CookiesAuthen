using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using CookiesAuthen.Application.Common.Interfaces.Repository;
using CookiesAuthen.Application.Common.Models;
using CookiesAuthen.Application.Common.Extentions;
using CookiesAuthen.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;

namespace CookiesAuthen.Infrastructure.Data.Persistence.Repositories;
public class Repository<T> : IRepository<T> where T : class
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public Repository(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        IMapper? mapper = null, int pageIndex = 1, int pageSize = 20, int rowModify = 0, bool disableTracking = true)
    {
        IQueryable<T> query = _context.Set<T>();
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        if (filter != null) query = query.Where(filter);
        if (orderBy != null) query = orderBy(query);

        return await GetPagedListAsync<TResult>(query, mapper ?? _mapper, pageIndex, pageSize, rowModify, disableTracking);
    }

    public async Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(
        IQueryable<T> query,
        IMapper? mapper = null, int pageIndex = 1, int pageSize = 20, int rowModify = 0, bool disableTracking = true)
    {
        var targetMapper = mapper ?? _mapper;

        var projectedQuery = query.ProjectTo<TResult>(targetMapper.ConfigurationProvider);

        return await projectedQuery.ToPagedListAsync(pageIndex, pageSize, rowModify);
    }
}
