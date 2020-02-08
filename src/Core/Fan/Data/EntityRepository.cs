using Fan.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        protected readonly bool isSqlite;

        /// <summary>
        /// The specific context initialized by sub class.
        /// </summary>
        private readonly DbContext _db;

        public EntityRepository(DbContext context) 
        {
            _entities = context.Set<T>();
            _db = context;
            isSqlite = _db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
        }

        /// <summary>
        /// Creates an entity and returns a tracked object with id.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>The <paramref name="entity"/> with id.</returns>
        /// <exception cref="FanException">
        /// Throws if insert violates unique key constraint. See <see cref="https://stackoverflow.com/a/47465944/32240"/>
        /// </exception>
        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                await _entities.AddAsync(entity);
                await _db.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateException dbUpdEx) 
            {
                throw GetExceptionForUniqueConstraint(dbUpdEx);
            }
        }

        /// <summary>
        /// Creates a list of entities.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
        {
            _entities.AddRange(entities);
            await _db.SaveChangesAsync();
            return entities;
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="id">The integer id of the entity.</param>
        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _entities.SingleAsync(e => e.Id == id);
            _entities.Remove(entity);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a list of objects of T based on search predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <remarks>
        /// Suitable when predicate is very simple and short.  If you take a look at 
        /// SqlTagRepository GetListAsync() that is not suitable for this.
        /// </remarks>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            isSqlite ? 
                _entities.ToList().Where(predicate.Compile()).ToList() :
                await _entities.Where(predicate).ToListAsync();

        /// <summary>
        /// Returns an object by id, returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<T> GetAsync(int id) => await _entities.SingleOrDefaultAsync(e => e.Id == id);

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entity">
        /// The entity to be updated, the EF implementation does not use this parameter.
        /// </param>
        /// <exception cref="FanException">
        /// Throws if update violates unique key constraint.
        /// </exception>
        public virtual async Task UpdateAsync(T entity)
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdEx)
            {
                throw GetExceptionForUniqueConstraint(dbUpdEx);
            }
        }

        /// <summary>
        /// Updates a list of entities.
        /// </summary>
        /// <param name="entities">
        /// The entities to be updated, the EF implementation does not use this parameter.
        /// </param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(IEnumerable<T> entities)
        {
            await _db.SaveChangesAsync();
        }

        private Exception GetExceptionForUniqueConstraint(DbUpdateException dbUpdEx)
        {
            if (dbUpdEx.InnerException != null)
            {
                var message = dbUpdEx.InnerException.Message;
                if (message.Contains("UniqueConstraint", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("Unique Constraint", StringComparison.OrdinalIgnoreCase))
                    return new FanException(EExceptionType.DuplicateRecord, dbUpdEx);

                if (dbUpdEx.InnerException.InnerException != null)
                {
                    message = dbUpdEx.InnerException.InnerException.Message;
                    if (message.Contains("UniqueConstraint", StringComparison.OrdinalIgnoreCase)
                        || message.Contains("Unique Constraint", StringComparison.OrdinalIgnoreCase))
                        return new FanException(EExceptionType.DuplicateRecord, dbUpdEx);
                }
            }

            return dbUpdEx;
        }
    }
}
