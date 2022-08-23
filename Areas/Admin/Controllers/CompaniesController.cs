
/*
Filename:  CompaniesController.cs
Purpose:   Control requests related to Company objects
Contains:  GET and POST actions methods to create, retrieve, update and delete companies
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-19
By:        Martin Dwyer
*/
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models;
using Microsoft.AspNetCore.Mvc;

namespace CC.Reads.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompaniesController : Controller
    {
        private readonly IUnitOfWork _context; // Adding Db context property

        /**
         * Constructor requires a live ApplicationDbContext from the repository
        */
        public CompaniesController(IUnitOfWork context)
        {
            _context = context;

        }


        /**
         * Action method to GET Company objects.  Takes in no parameters.  Is async to wait
         * for data retrieval. 
         *
         * Returns a View with a list of Company objects.
        */
        public IActionResult Index()
        {

            return View();
        }


        /**
         * Details(int) action method handles a GET request for Company details.  The method accepts 
         * an integer, retrieves the company object from the database, and sends the object to a view 
         * where it is displayed.
         * 
         * param id the unique identifier of the Company object
         * returns a View with the category object          
        */
        public IActionResult Details(int? id)
        {

            var obj = _context.Company.GetFirstOrDefault(m => m.Id == id, "");
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }


        /**
         * Upsert(int id) handles a GET request to update or insert a Company object with id as the
         * unique identifier for the object.  The method checks to see if the Company object can be found with 
         * the provided id.  If found, a View is presented with the Company object information.  If not found,
         * a View is presented with an empty Company object.
         * 
         * param id, an int, is the unique identifier for the Company object
         * returns a View with the Company object if found in the Db
         * returns a View with an empty Company object if not found in the Db
        */
        public IActionResult Upsert(int? id)
        {
            Company company = new();

            // If id not provided or 0, return a View with the empty company object
            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                // Otherwise, return a view with the company object found
                company = _context.Company.GetFirstOrDefault(u => u.Id == id, "");

                return View(company);
            }

        }


        /**
         * Upsert(Company company) handles a POST request to update or insert a Company object.
         * The method handles Model validation and either creates or updates a Company object as appropriate.
         * 
         * param company, a Company object to be inserted or updated  
         * returns a View with a list of all Company objects
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {

            if (company.Id == 0)
            {
                _context.Company.Add(company);
                TempData["success"] = "Company created successfully";
            }
            else
            {
                _context.Company.Update(company);
                TempData["success"] = "Company updated successfully";
            }

            _context.Save();

            return RedirectToAction("Index");

        }

        /*
         * API end points allow the developer to create, retrieve, update or delete data 
         * in the Db without updating the Page or View.  
         */
        #region API CALLS
        
        /**
         * GetAll() handles a GET request for all objects.  It returns all Company objects in
         * Json format.
         */
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _context.Company.GetAll();
            return Json(new { data = companyList });

        }

        /**
         * Delete(int id) handles a POST request to delete Company object with unique identifier of
         * id.  If successful, a Json object is returned indicating the object was deleted
         */
        [HttpDelete]
        public IActionResult Delete(int id)
        {

            var obj = _context.Company.GetFirstOrDefault(m => m.Id == id, "");

            _context.Company.Remove(obj);
            _context.Save();


            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }



}
