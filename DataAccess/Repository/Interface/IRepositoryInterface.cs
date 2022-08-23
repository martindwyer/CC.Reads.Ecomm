/*
Filename:  IRepositoryInterface.cs
Purpose:   Provide the base functionality for repository functions 
Contains:  Requirements and results for functions: GetAll, Add, GetFirstOrDefault, Remove, RemoveRange
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using System.Linq.Expressions;

namespace CC.Reads.DataAccess.Repository.Interface
{
    public interface IRepositoryInterface<T>
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        void Add(T entity);
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string includeProperties = "");
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
