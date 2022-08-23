/*
Filename:  CoverTypesController.cs
Purpose:   Control requests related to CoverType objects
Contains:  GET and POST actions methods to create, retrieve, update and delete cover types
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
    [Area("Admin")]  // Specifying the area for routing purposes
    public class CoverTypesController : Controller
    {
        private readonly ApplicationDbContext _context;  // Adding Db context property


        /**
         * Constructor requires a live ApplicationDbContext from the repository
        */
        public CoverTypesController(ApplicationDbContext context)
        {
            _context = context;
        }


        /**
         * Action method to GET all CoverType objects.  Takes in no parameters.  Is async to wait
         * for data retrieval. 
         *
         * Returns a View with a list of CoverType objects.
        */
        public async Task<IActionResult> Index()
        {
            return _context.CoverTypes != null ?
                        View(await _context.CoverTypes.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.CoverTypes'  is null.");
        }


        /**
         * Details(int) action method handles a GET request for CoverType object details.  The method accepts 
         * an integer, retrieves the CoverType object from the database, and sends the object to a view 
         * where it is displayed.
         * 
         * param id the unique identifier of the CoverType object
         * returns a View with the CoverType object        
         */
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CoverTypes == null)
            {
                return NotFound();
            }

            var coverType = await _context.CoverTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        /**
         * Create() handles a GET request to create a CoverType object.  The method accepts no 
         * parameters.  The method returns a View with an appropriate form for creating the 
         * object.  
         * 
         * returns a View object for CoverType creation
         */
        public IActionResult Create()
        {
            return View();
        }


        /**
         * Create(CoverType covertype) method handles a POST request to create a CoverType object.  
         *
         * param Id, an integer, represents the CoverType Id
         * param Name, a string, represents the CoverType Name
         *
         * returns the CoverType list if creation successful
         * returns to the Create view if Model is not valid        
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] CoverType coverType)
        {
            // Check the model state
            if (ModelState.IsValid)
            {
                // Add covertype to Db context
                _context.Add(coverType);
                await _context.SaveChangesAsync();

                // Return to CoverType list
                return RedirectToAction(nameof(Index));
            }

            // If Model not valid return to CoverType Create() View
            return View(coverType);
        }


        /**
         * Edit(int id) handles a GET request to edit a CoverType object
         * 
         * param id, an integer, is the unique identifier for the CoverType object
         * returns an Edit View with the CoverType object if found
         * returns NotFound View if the CoverType object could not be found
        */
        public async Task<IActionResult> Edit(int? id)
        {
            // If no id provided, return NotFound
            if (id == null || _context.CoverTypes == null)
            {
                return NotFound();
            }

            // Try to retrieve the covertype with the provided id
            var coverType = await _context.CoverTypes.FindAsync(id);

            // If no covertype can be found, return NotFound()
            if (coverType == null)
            {
                return NotFound();
            }

            // Return Edit View with coverType object
            return View(coverType);
        }


        /*
         * Edit(category) handles a POST request to edit a CoverType object
         *
         * param coverType is a CoverType object as defined by the User in the Edit View
         * returns the CoverType list Index View if CoverType object found and updated
         * returns NotFound View if the CoverType object could not be found in Db
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] CoverType coverType)
        {

            // If there is an Id mismatch, return NotFound()
            if (id != coverType.Id)
            {
                return NotFound();
            }


            // If model state is valid, update CoverType object in Db
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coverType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoverTypeExists(coverType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Return to CoverType Index View listing all cover types
                return RedirectToAction(nameof(Index));
            }

            // If model is invalid return to Edit View with coverType object
            return View(coverType);
        }

        /**
         * Delete(int id) handles a GET request to delete a CoverType object
         * 
         * param id, an integer, is the unique identifier for the CoverType object
         * returns a Delete View with the CoverType object if found
         * returns NotFound View if the CoverType object could not be found
        */
        public async Task<IActionResult> Delete(int? id)
        {
            // If id is not provided return NotFound()
            if (id == null || _context.CoverTypes == null)
            {
                return NotFound();
            }

            // Attempt to retrieve CoverType with id
            var coverType = await _context.CoverTypes
                .FirstOrDefaultAsync(m => m.Id == id);

            // Return NotFound() if CoverType object not found
            if (coverType == null)
            {
                return NotFound();
            }

            // Return the Delete View with the CoverType object
            return View(coverType);
        }

        /**
         * Delete(CoverType coverType) handles a POST request to delete a CoverType object.
         * 
         * param id, an integer, is the unique identifier of the CoverType object
         * returns the CoverType list Index View if CoverType object found and deleted
         * returns NotFound View if the CoverType object could not be found in Db
        */
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CoverTypes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CoverTypes'  is null.");
            }

            // Attempt to retrieve the CoverType object with id
            var coverType = await _context.CoverTypes.FindAsync(id);

            // Remove CoverType object from Db if exists
            if (coverType != null)
            {
                _context.CoverTypes.Remove(coverType);
            }
            await _context.SaveChangesAsync();

            // Return to CoverType list
            return RedirectToAction(nameof(Index));
        }


        /**
         * CoverTypeExists(int id) takes an integer and returns a boolean
         * 
         * returns true if id exists as a CoverType Id in the Db
         * returns false if id does not exist as category Id in the Db
        */
        private bool CoverTypeExists(int id)
        {
            return (_context.CoverTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
