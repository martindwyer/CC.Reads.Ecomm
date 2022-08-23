/*
Filename:  CoverTypeRepository.cs
Purpose:   Provide the implementation of ICoverTypeInterface.  
Contains:  Implements ICoverTypeInterface.  Extends Repository 
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/


using CC.Reads.DataAccess.Data;
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models;

namespace CC.Reads.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {

        private readonly ApplicationDbContext _db;

        public CoverTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(CoverType coverType)
        {
            _db.CoverTypes.Update(coverType);
        }
    }
}
