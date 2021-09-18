using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Domain.Search;

namespace WebApiDemo.Domain.Repositories
{
    public interface IRepository<TEntity, TKey> 
        where TEntity : class, new()
    {
        Task<List<TEntity>> GetAll(string sortOrder);

        Task<TEntity> GetByKey(TKey key);
        
        Task<(List<TEntity> SearchResult, bool IsSearchOptionValid)> Search(SearchOption searchOption, string sortOrder);

        Task<TEntity> Add(TEntity entity);

        Task<TEntity> Update(TEntity entity);
    }
}
