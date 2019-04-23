using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Contract for a base repository that provides commonly used data access methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Common implementations of this interface could be sql or no-sql databases.
    /// See <see cref="EntityRepository{T}"/> for an Entity Framework implementation.
    /// 
    /// Only the common-denominator operations are here, for example Delete is not here due to the
    /// fact deletion needs different logic for different entities.
    /// </remarks>
    public interface IRepository<T> where T : class 
    {
        /// <summary>
        /// Creates an object in data source.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<T> CreateAsync(T obj);

        /// <summary>
        /// Creates objects in data source.
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> objs);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="id">The integer id of the entity.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns objects found from data source based on search predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets an object by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync(int id);

        /// <summary>
        /// Updates an object in data source.
        /// </summary>
        /// <param name="obj">
        /// The object to be updated, not all implementations need this parameter. 
        /// The EF implementation does not while others may.
        /// </param>
        /// <returns></returns>
        Task UpdateAsync(T obj);

        /// <summary>
        /// Updates some objects in data source.
        /// </summary>
        /// <param name="objs">
        /// The objects to be updated, not all implementations need this parameter. 
        /// The EF implementation does not while others may.
        /// </param>
        /// <returns></returns>
        Task UpdateAsync(IEnumerable<T> objs);
    }
}
