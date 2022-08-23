/*
Filename:  CompanyRepository.cs
Purpose:   Provide the implementation of ICompanyInterface.  
Contains:  Implements ICompanyInterface.  Extends Repository 
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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Company company)
        {
            _db.Companies.Update(company);
        }
    }
}