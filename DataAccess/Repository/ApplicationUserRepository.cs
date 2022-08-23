/*
Filename:  ApplicationUserRepository.cs
Purpose:   Provide the implementation of IApplicationUserInterface.  
Contains:  Implements IApplicationUserInterface.  Extends Repository 
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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {

        private readonly ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


    }
}
