/*
Filename:  IOrderHeaderRepository.cs
Purpose:   Provide the base functionality for OrderHeaderRepository 
Contains:  Inherits the functionality of IRepositoryInterface and defines a unique methods for OrderHeaders
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.Models;
using System.Linq.Expressions;

namespace CC.Reads.DataAccess.Repository.Interface
{
    public interface IOrderHeaderRepository : IRepositoryInterface<OrderHeader>
    {
        void Update(OrderHeader obj);

        // Method to update order and payment status
        void UpdateStatus(int id, string orderStatus, string paymentStatus = null);

        // Method to get all with filters and properties
        IEnumerable<OrderHeader> GetAll(Expression<Func<OrderHeader, bool>> filter, string includeProperties = "");

        // Method to update Stripe payment id
        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
    }
}
