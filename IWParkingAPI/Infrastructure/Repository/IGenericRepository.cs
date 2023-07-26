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
    }
}
