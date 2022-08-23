/*
Filename:  IOrderDetailRepository.cs
Purpose:   Provide the base functionality for OrderDetailRepository 
Contains:  Inherits the functionality of IRepositoryInterface and defines a unique GetAll method
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.Models;
using System.Linq.Expressions;

namespace CC.Reads.DataAccess.Repository.Interface
{
    public interface IOrderDetailRepository : IRepositoryInterface<OrderDetail>
    {
        void Update(OrderDetail obj);

        // Unique method for OrderDetail Repository
        IEnumerable<OrderDetail> GetAll(Expression<Func<OrderDetail, bool>> filter, string includeProperties = "");
    }
}
