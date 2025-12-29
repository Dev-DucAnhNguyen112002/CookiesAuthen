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

    Task<T?> GetByIdAsync(object id);

    Task<List<T>> GetByIdsAsync<TKey>(IEnumerable<TKey> ids, Expression<Func<T, TKey>> keySelector);

    Task<IReadOnlyList<T>> GetAllAsync();

    Task<T> AddAsync(T entity);

    /// <summary>

    /// Kiểm tra tồn tại trong db

    /// có thể loại bỏ 1 bản ghi theo id

    /// </summary>

    /// <param name="propertyName">Tên trường muốn kiểm tra</param>

    /// <param name="propertyValue">Giá trị muốn kiểm tra</param>

    /// <param name="idFieldName">Tên khóa chính</param>

    /// <param name="idValue">giá trị khóa chính</param>

    /// <returns></returns>

    /// 

    Task<bool> ExistsByPropertyAsync<TType>(string propertyName, string propertyValue, string? idFieldName = null, TType? idValue = default);

    void Update(T entity);

    void Delete(T entity);

    void UpdateRange(IEnumerable<T> entities);

    Task AddRangeAsync(IEnumerable<T> entities);

    void DeleteRange(IEnumerable<T> entities);

    void SoftDeleteRange(IEnumerable<T> entities, string? username = null);





    /// <summary>

    /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method defaults to a read-only, no-tracking query.

    /// </summary>

    /// <param name="predicate">A function to test each element for a condition.</param>

    /// <param name="orderBy">A function to order elements.</param>

    /// <param name="include">A function to include navigation properties</param>

    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>

    /// <returns>An <see cref="IPagedList{T}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>

    /// <remarks>This method defaults to a read-only, no-tracking query.</remarks>

    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null,

        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,

        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,

        bool disableTracking = true);



    /// <summary>

    /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method default no-tracking query.

    /// </summary>

    /// <param name="predicate">A function to test each element for a condition.</param>

    /// <param name="orderBy">A function to order elements.</param>

    /// <param name="include">A function to include navigation properties</param>

    /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>

    /// <returns>An <see cref="IPagedList{T}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>

    /// <remarks>This method default no-tracking query.</remarks>

    /// <typeparam name="TResult"></typeparam>

    /// <param name="mapper"></param>

    /// <returns></returns>

    TResult? GetFirstOrDefault<TResult>(Expression<Func<T, bool>>? predicate = null,

        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,

        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,

        bool disableTracking = true,

        IMapper? mapper = null);



    /// <summary>

    /// GetQueryable

    /// </summary>

    /// <param name="filter"></param>

    /// <param name="orderBy"></param>

    /// <param name="include"></param>

    /// <param name="disableTracking"></param>

    /// <returns></returns>

    IQueryable<T> GetQueryable(Expression<Func<T, bool>>? filter = null,

        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,

        Func<IQueryable<T>, IQueryable<T>>? include = null,

        bool disableTracking = true);



    /// <summary>

    /// ToPagedListAsync<TResult>

    /// </summary>

    /// <typeparam name="TResult"></typeparam>

    /// <param name="filter"></param>

    /// <param name="orderBy"></param>

    /// <param name="include"></param>

    /// <param name="mapper"></param>

    /// <param name="pageIndex"></param>

    /// <param name="pageSize"></param>

    /// <param name="rowModify"></param>

    /// <param name="disableTracking"></param>

    /// <returns></returns>

    // Hàm 1: Dành cho AutoMapper (Truyền Entity vào, Repository tự Map sang TResult)
    Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        IMapper? mapper = null,
        int pageIndex = 1,
        int pageSize = 20,
        int rowModify = 0,
        bool disableTracking = true);

    Task<PaginatedList<TResult>> GetPagedListAsync<TResult>(
        IQueryable<TResult> query,
        int pageIndex = 1,
        int pageSize = 20,
        int rowModify = 0);

    /// <summary>

    /// ApplyOrdering

    /// </summary>

    /// <param name="query"></param>

    /// <param name="sortBy"></param>

    /// <param name="isDesc"></param>

    /// <typeparam name="TEntity"></typeparam>

    /// <returns></returns>

    IQueryable<TEntity> ApplyOrdering<TEntity>(IQueryable<TEntity> query, string? sortBy, bool? isDesc) where TEntity : class;

}
