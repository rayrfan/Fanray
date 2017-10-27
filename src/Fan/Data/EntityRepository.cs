using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// EF implementation of a base repository for commonly used data access methods.
    /// </summary>
    /// <typeparam name="T">It is a sub type of <see cref="Entity"/> which provides an int based PK.</typeparam>
    /// <remarks>
    /// The sub class will have its specific methods, for example SqlCategoryRepository and SqlTagRepository
    /// has a GetListAsync() which does their specific join.
    /// </remarks>
    public class EntityRepository<T> : IRepository<T> where T : Entity
    {
        /// <summary>
        /// The set initialized by sub class.
        /// </summary>
        protected readonly DbSet<T> _entities;
        /// <summary>
        /// The specific context initialized by sub class.
        /// </summary>
        private readonly DbContext _db;

        public EntityRepository(DbContext context) 
        {
            _entities = context.Set<T>();
            _db = context;
        }

        /// <summary>
        /// Creates an entity and the returned object is tracked.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="DbUpdateException">
        /// If there is a unique constraint and adding a new record with same key, such as <see cref="Fan.Models.Meta"/>.
        /// </exception>
        public virtual async Task<T> CreateAsync(T entity)
        {
            await _entities.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entity">Not all implementations use this parameter, such as the Sql ones.</param>
        /// <returns></returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
