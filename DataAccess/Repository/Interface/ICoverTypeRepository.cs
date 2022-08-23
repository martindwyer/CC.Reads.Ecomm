/*
Filename:  ICoverTypeRepository.cs
Purpose:   Provide the base functionality for CoverTypeRepository 
Contains:  Inherits the functionality of IRepositoryInterface
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.Models;

namespace CC.Reads.DataAccess.Repository.Interface
{
    public interface ICoverTypeRepository : IRepositoryInterface<CoverType>
    {
        void Update(CoverType coverType);
    }
}