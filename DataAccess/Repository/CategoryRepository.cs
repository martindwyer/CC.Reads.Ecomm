/*
Filename:  CategoryRepository.cs
Purpose:   Provide the implementation of ICategoryInterface.  
Contains:  Implements ICategoryInterface.  Extends Repository 
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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {

        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
