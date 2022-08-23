/*
Filename:  ShoppingCartRepository.cs
Purpose:   Provide the implementation of IShoppingCartInterface.  
Contains:  Implements IShoppingCartInterface.  Extends Repository 
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.DataAccess.Data;
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CC.Reads.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public int DecrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count -= count;
            return (int)shoppingCart.Count;
        }

        public int IncrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;
            return (int)shoppingCart.Count;
        }

        public IEnumerable<ShoppingCart> GetAll(Expression<Func<ShoppingCart, bool>> filter, string includeProperties = "")
        {
            IQueryable<ShoppingCart> query = DbSet;
            query = query.Where(filter);
            if (includeProperties != "")
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();

        }
    }

}
