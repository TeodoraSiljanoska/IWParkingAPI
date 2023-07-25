using Microsoft.EntityFrameworkCore;

namespace IWParkingAPI.Infrastructure.Repository
{
    public class SQLRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private DbContext _context;
        private DbSet<TEntity> _db;

        public SQLRepository(DbContext context)
        {
            _context = context;
            _db = context.Set<TEntity>();
        }
        public void Delete(TEntity obj)
        {
            _db.Remove(obj);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _db.ToList();
        }

        public TEntity GetById(object id)
        {
            return _db.Find(id);
        }

        public void Insert(TEntity obj)
        {
            _db.Add(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(TEntity obj, TEntity objChanges)
        {
            _db.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
        }
    }
}