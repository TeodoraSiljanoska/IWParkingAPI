using IWParkingAPI.Infrastructure.Repository;

namespace IWParkingAPI.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork<TContext>
    {
        void CreateTransaction();
        void Commit();
        void Rollback();
        void Save();

        IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class;
    }
}
