/*
Filename:  CategoriesController.cs
Purpose:   Control requests related to Category objects
Contains:  GET and POST actions methods to create, retrieve, update and delete categories
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-19
By:        Martin Dwyer
*/
using CC.Reads.DataAccess.Data;
using CC.Reads.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CC.Reads.Areas.Admin.Controllers
{
    [Area("Admin")] // Specifying the area for routing purposes
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;  // Adding Db context property

        /**
         * Constructor requires a live ApplicationDbContext from the repository
        */
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /**
         * Action method to GET categories.  Takes in no parameters.  Is async to wait
         * for data retrieval. 
         *
         * Returns a View with a list of Category objects.
        */
        public async Task<IActionResult> Index()
        {
            // Wait on retrieval of data and then return View with data
            return _context.Categories != null ?
                        View(await _context.Categories.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
        }


        /**
         * Details(int) action method handles a GET request for category details.  The method accepts 
         * an integer, retrieves the category object from the database, and sends the object to a view 
         * where it is displayed.
         * 
         * param id the unique identifier of the Category object
         * returns a View with the category object          
        */
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        /**
         * Create() handles a GET request to create a Category object.  The method accepts no 
         * parameters.  The method returns a View with an appropriate form for creating the 
         * object.  
         * 
         * returns a View object for Category creation    
        */
        public IActionResult Create()
        {
            return View();
        }

        /**
         * Create method handles a POST request to create a Category object.  
         *
         * param Id, an integer, represents the Category Id
         * param Name, a string, represents the Category Name.
         * param DisplayOrder, an integer, represents the display order
         * param CreatedDate, a DateTime object, is the category's created date
         *
         * returns the Category list if creation successful
         * returns to the Create view if Model is not valid        
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DisplayOrder,CreatedDate")] Category category)
        {
            // Check the model state
            if (ModelState.IsValid)
            {
                // Add category to Db context 
                _context.Add(category);
                await _context.SaveChangesAsync();

                // Return to Category list
                return RedirectToAction(nameof(Index));
            }

            // If not valid return to the Create view with the category object
            return View(category);
        }


        /**
         * Edit(int id) handles a GET request to edit a category
         * 
         * param id, an integer, is the unique identifier for the category object
         * returns an Edit View with the category object if found
         * returns NotFound View if the category object could not be found
        */
        public async Task<IActionResult> Edit(int? id)
        {
            // If no id provided, return NotFound
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            // Try to retrieve the category with the provided id
            var category = await _context.Categories.FindAsync(id);

            // If no category object could be found, return NotFound
            if (category == null)
            {
                return NotFound();
            }

            // Return Edit View with category object
            return View(category);
        }


        /*
         * Edit(category) handles a POST request to edit a category
         *
         * param category is a category object as defined by the User in the Edit View
         * returns the Category list Index View if category object found and updated
         * returns NotFound View if the category object could not be found in Db
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DisplayOrder,CreatedDate")] Category category)
        {
            // If there is an Id mismatch, return NotFound
            if (id != category.Id)
            {
                return NotFound();
            }

            // If model state valid, update category record in Db 
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Return to Category List View
                return RedirectToAction(nameof(Index));
            }

            // For Model state invalid return to Edit View with category
            return View(category);
        }

        /**
         * Delete(int id) handles a GET request to delete a category
         * 
         * param id, an integer, is the unique identifier for the category object
         * returns a Delete View with the category object if found
         * returns NotFound View if the category object could not be found
        */
        public async Task<IActionResult> Delete(int? id)
        {
            // If id not provided return NotFound
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            // Attempt to retrieve category with id
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            // Return NotFound if category object could not be found
            if (category == null)
            {
                return NotFound();
            }

            // Return the Delete View with the category object
            return View(category);
        }


        /**
         * Delete(Category category) handles a POST request to delete a category
         * 
         * param id, an integer, is the unique identifier of the category object
         * returns the Category list Index View if category object found and deleted
         * returns NotFound View if the category object could not be found in Db
        */
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }

            // Attempt to retrieve category object with id
            var category = await _context.Categories.FindAsync(id);

            // Remove category from Db if found
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            await _context.SaveChangesAsync();

            // Return to Category list
            return RedirectToAction(nameof(Index));
        }

        /**
         * CategoryExists(int id) takes an integer and returns a boolean.
         * 
         * returns true if id exists as category Id in the Db
         * returns false if id does not exist as category Id in the Db
        */
        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
