﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

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

        public void Update(TEntity obj)
        {
            _db.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
        }

        public bool FindByPredicate(Func<TEntity, bool> predicate)
        {
            return _db.Any(predicate);
        }

       public virtual IQueryable<TEntity> GetAsQueryable(
           Expression<Func<TEntity, bool>>? filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
           Expression<Func<TEntity, object>>? orderProperty = null,
           bool isDescending = false)
        {
            IQueryable<TEntity> query = _db;

            if(filter != null)
            {
                query = query.Where(filter);
            }

            if(include != null)
            {
                query = include(query);
            }

            if (orderBy != null)
            {
                if (isDescending)
                {
                    return orderBy(query).OrderByDescending(orderProperty);
                }
                else
                {
                    return orderBy(query).OrderBy(orderProperty);
                }
            }

            else
            {
                return query;
            }
        }

       
    }
}