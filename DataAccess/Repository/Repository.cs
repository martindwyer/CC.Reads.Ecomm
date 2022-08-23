/*
Filename:  Repository.cs
Purpose:   Provide the implementation of IRepositoryInterface.  This implementation utilizes the Entity Framework
           syntax for functionality.  However, if Entity Framework was not the ORM of choice, the code for Db
           manipulation would change only here.           
Contains:  Implementation of IRepositoryInterface with Entity Framework syntax 
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.DataAccess.Data;
using CC.Reads.DataAccess.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CC.Reads.DataAccess.Repository
{
    public class Repository<T> : IRepositoryInterface<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> DbSet;

        // Constructor makes an instance of the ApplicationDbContext available
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.DbSet = _db.Set<T>();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = "")
        {
            IQueryable<T> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public void Add(T entity)
        {
            this.DbSet.Add(entity);
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string includeProperties = "")
        {
            IQueryable<T> query = DbSet;
            query = query.Where(filter);
            if (includeProperties != "")
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            this.DbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            this.DbSet.RemoveRange(entities);
        }
    }
}
