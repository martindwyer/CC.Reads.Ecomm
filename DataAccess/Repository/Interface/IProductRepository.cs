/*
Filename:  IProductRepository.cs
Purpose:   Provide the base functionality for ProductRepository 
Contains:  Inherits the functionality of IRepositoryInterface
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.Models;

namespace CC.Reads.DataAccess.Repository.Interface
{
    public interface IProductRepository : IRepositoryInterface<Product>
    {
        void Update(Product product);
    }
}