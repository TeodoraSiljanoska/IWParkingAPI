namespace IWParkingAPI.Infrastructure.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        TEntity GetById(object id);
        void Insert(TEntity obj);
        void Update(TEntity obj, TEntity objChanges);
        void Delete(TEntity id);
        void Save();
    }
}
