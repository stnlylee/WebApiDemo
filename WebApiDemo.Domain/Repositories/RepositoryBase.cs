using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Domain.Search;

namespace WebApiDemo.Domain.Repositories
{
    /// <summary>
    /// Normally this class should implement all common CRUD functions for database
    /// eg, EntityFramework can work with generic entity 
    /// But for the demo MovieDataSource is not generic so I have to implement in child class
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class RepositoryBase<TEntity, TKey> 
        : IRepository<TEntity, TKey>
        where TEntity : class, new()
    {
        public virtual Task<List<TEntity>> GetAll(string sortOrder)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TEntity> GetByKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public virtual Task<(List<TEntity> SearchResult, bool IsSearchOptionValid)> Search(SearchOption searchOption, string sortOrder)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TEntity> Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TEntity> Update(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
