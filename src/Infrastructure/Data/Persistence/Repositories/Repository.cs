
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CookiesAuthen.Application.Common.Extentions;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Interfaces.Repository;
using CookiesAuthen.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
namespace CookiesAuthen.Infrastructure.Data.Persistence.Repositories;
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly DbSet<T> _dbSet;

    public Repository(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        // Vì IApplicationDbContext của bạn có hàm Set<T>(), ta dùng nó để lấy DbSet
        _dbSet = _context.Set<T>();
    }
    public async Task<T?> GetFirstOrDefaultAsync(
    Expression<Func<T, bool>>? predicate = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
    bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        if (predicate != null) query = query.Where(predicate);

        if (orderBy != null)
            return await orderBy(query).FirstOrDefaultAsync();

        return await query.FirstOrDefaultAsync();
    }

    // 2. Triển khai bản trả về TResult (Dto) sử dụng AutoMapper
    public TResult? GetFirstOrDefault<TResult>(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool disableTracking = true,
        IMapper? mapper = null)
    {
        IQueryable<T> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        if (predicate != null) query = query.Where(predicate);

        var targetMapper = mapper ?? _mapper;
        var projectedQuery = query.ProjectTo<TResult>(targetMapper.ConfigurationProvider);

        if (orderBy != null)
        {
            // Lưu ý: OrderBy trên TResult có thể khác OrderBy trên T
            // Ở đây ta ép kiểu orderBy cho query gốc trước khi project
            return orderBy(query).ProjectTo<TResult>(targetMapper.ConfigurationProvider).FirstOrDefault();
        }

        return projectedQuery.FirstOrDefault();
    }
    public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

    public async Task<List<T>> GetByIdsAsync<TKey>(IEnumerable<TKey> ids, Expression<Func<T, TKey>> keySelector)
    {
        return await _dbSet.Where(x => ids.Contains(keySelector.Compile()(x))).ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

    public void Update(T entity) => _dbSet.Update(entity);

    public void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    public async Task<bool> ExistsByPropertyAsync<TType>(string propertyName, string propertyValue, string? idFieldName = null, TType? idValue = default)
    {
        // Tạo dynamic query: x => x.PropertyName == propertyValue && x.Id != idValue
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var value = Expression.Constant(propertyValue);
        var equality = Expression.Equal(property, value);

        if (!string.IsNullOrEmpty(idFieldName) && idValue != null)
        {
            var idProperty = Expression.Property(parameter, idFieldName);
            var idConstant = Expression.Constant(idValue);
            var idInequality = Expression.NotEqual(idProperty, idConstant);
            equality = Expression.AndAlso(equality, idInequality);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
        return await _dbSet.AnyAsync(lambda);
    }

    public IQueryable<T> GetQueryable(Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        if (filter != null) query = query.Where(filter);
        if (orderBy != null) query = orderBy(query);
        return query;
    }

    public async Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(
     Expression<Func<T, bool>>? filter = null,
     Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
     Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
     IMapper? mapper = null, int pageIndex = 1, int pageSize = 20, int rowModify = 0, bool disableTracking = true)
    {
        IQueryable<T> query = _dbSet;
        if (disableTracking) query = query.AsNoTracking();
        if (include != null) query = include(query);
        if (filter != null) query = query.Where(filter);
        if (orderBy != null) query = orderBy(query);

        // Chuyển sang dùng Mapper để Project trước khi gọi hàm phân trang thuần túy
        var targetMapper = mapper ?? _mapper;
        var projectedQuery = query.ProjectTo<TResult>(targetMapper.ConfigurationProvider);

        return await GetPagedListAsync(projectedQuery, pageIndex, pageSize);
    }

    // Thêm vào GenericRepository
    public async Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(
    IQueryable<TResult> query,
    int pageIndex = 1,
    int pageSize = 20,
    int rowModify = 0)
    {
        // 1. Đếm tổng số bản ghi từ query đã được map
        var count = await query.CountAsync();

        // 2. Thực hiện phân trang trên dữ liệu TResult
        var items = await query
            .Skip((pageIndex - 1) * pageSize + rowModify)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<TResult>(items, count, pageIndex, pageSize);
    }

    public IQueryable<TEntity> ApplyOrdering<TEntity>(IQueryable<TEntity> query, string? sortBy, bool? isDesc) where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return query;

        string orderCommand = isDesc == true ? $"{sortBy} descending" : $"{sortBy} ascending";
        return DynamicQueryableExtensions.OrderBy(query, orderCommand);
    }

    public void SoftDeleteRange(IEnumerable<T> entities, string? username = null)
    {
        foreach (var entity in entities)
        {
            // Giả định entity có interface ISoftDelete hoặc trường IsDeleted
            var property = entity.GetType().GetProperty("IsDeleted");
            if (property != null) property.SetValue(entity, true);
        }
        _dbSet.UpdateRange(entities);
    }
}
