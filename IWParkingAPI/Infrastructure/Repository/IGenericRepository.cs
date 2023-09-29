using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace IWParkingAPI.Infrastructure.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        TEntity GetById(object id);
        void Insert(TEntity obj);
        void Update(TEntity obj);
        void Delete(TEntity id);
        void Save();
        bool FindByPredicate(Func<TEntity, bool> predicate);

        IQueryable<TEntity> GetAsQueryable(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            Expression<Func<TEntity, object>>? orderProperty = null,
            bool isDescending = false);
    }
}
