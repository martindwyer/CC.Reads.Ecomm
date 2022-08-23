/*
Filename:  IShoppingCartRepository.cs
Purpose:   Provide the base functionality for ShoppingCartRepository 
Contains:  Inherits the functionality of IRepositoryInterface and defines unique methods for ShoppingCartRepository
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/
using CC.Reads.Models;
using System.Linq.Expressions;

namespace CC.Reads.DataAccess.Repository.Interface
{

    public interface IShoppingCartRepository : IRepositoryInterface<ShoppingCart>
    {
        // Unique method to increment count in shopping carts
        int IncrementCount(ShoppingCart shoppingCart, int count);
        
        // Unique method to decrement count in shopping carts
        int DecrementCount(ShoppingCart shoppingCart, int count);

        // Unique method to get all with filters and properties
        IEnumerable<ShoppingCart> GetAll(Expression<Func<ShoppingCart, bool>> filter, string includeProperties = "");

    }
}
