/*
Filename:  ProductsController.cs
Purpose:   Control requests related to Product objects
Contains:  GET and POST actions methods to create, retrieve, update and delete Product objects
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-22
By:        Martin Dwyer
*/
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CC.Reads.Areas.Admin.Controllers
{
    [Area("Admin")] // Specifying the area for routing purposes
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _context;                  // Db Context for data access
        private readonly IWebHostEnvironment _hostEnvironment;  // Web context to use path for file saving
        
        /**
         * Constructor requires a new Db context and host environment
        */
        public ProductsController(IUnitOfWork context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        /**
         * Action method to GET all Product objects.  Takes in no parameters.  Data is retrieved
         * and placed into data tables on with JavaScript in the View.
         * 
         * Returns a View with JavaScript to retrieve and display products.
        */
        public IActionResult Index()
        {
            return View();
        }

        /**
         * Details(int id) action method handles a GET request for Product object details.  The method accepts 
         * an integer, retrieves the Product object from the Db.  The Product object is returned with Details View.
         * 
         * param id the unique identifier of the Product object
         * returns a View with the Product object        
         */
        public IActionResult Details(int? id)
        {
            // Retrieve the Product object from the Db
            var obj = _context.Product.GetFirstOrDefault(m => m.Id == id, "Category,CoverType");
            
            // Return View with Product object
            return View(obj);
        }


        /**
         * Upsert(int id) handles a GET request to update or insert a Product object with id as the
         * unique identifier for the object.  The method checks to see if the Product object can be found with 
         * the provided id.  If found, a View is presented with the Product object information.  If not found,
         * a View is presented with an empty Product object.
         * 
         * param id, an int, is the unique identifier for the Product object
         * returns a View with the Product object if found in the Db
         * returns a View with an empty Product object if not found in the Db
        */
        public IActionResult Upsert(int? id)
        {
            // Create a new ProductViewModel with an empty Product object
            ProductViewModel pvm = new()
            {
                Product = new(),
                CategoryList = _context.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                CoverTypeList = _context.CoverType.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
            };

            // Return the model with empty Product object if no id or id is zero
            if (id == null || id == 0)
            {
                return View(pvm);
            }
            else
            {
                // Retrieve the product object with the id and update the ProductViewModel
                pvm.Product = _context.Product.GetFirstOrDefault(u => u.Id == id, "");
                return View(pvm);
            }

        }

        /**
         * Upsert(ProductViewModel pvm, IFormFile file) handles a POST request to update or insert a Product object.
         * The method handles Model validation and either creates or updates a Product object as appropriate.
         * 
         * param pvm, a ProductViewModel object containing a Product to be inserted or updated  
         * returns a View with a list of all Product objects
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel pvm, IFormFile file)
        {
            // No Product.  No dice.
            if (pvm.Product == null)
            {
                return NotFound();
            }

            // Model validation
            if (ModelState.IsValid)
            {
                // Using web environment to get the current root path
                string wwwPath = _hostEnvironment.WebRootPath;

                // Process photo for saving to server if photo file exists
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();                     // New name for file
                    var uploads = Path.Combine(wwwPath, @"images/products");    // Path to save file
                    var extension = Path.GetExtension(file.FileName);           // Extension for file

                    // Delete existing file if it exists
                    if (pvm.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwPath, pvm.Product.ImageUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Create and use a FileStream object to save the photo
                    using (var fileStreams =
                           new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }

                    // Set the product image url
                    pvm.Product.ImageUrl = @"/images/products/" + fileName + extension;
                }
                
                // Add product to Db
                try
                {
                    if (pvm.Product.Id == 0)
                    {
                        _context.Product.Add(pvm.Product);
                        TempData["success"] = "Product created successfully";
                    }
                    else
                    {
                        _context.Product.Update(pvm.Product);
                        TempData["success"] = "Product updated successfully";
                    }
                    _context.Save();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                
                // Return the Index view with all products once the product is saved
                return RedirectToAction(nameof(Index));
            }
            
            // Return if Model does not validate
            return View(pvm);
        }

        /*
         * The following section provides REST API routes so that products can be retrieved, edited, or
         * deleted with JavaScript from the client side View.
         * 
         */
        #region API CALLS
        
        /*
         * The GetAll() route returns all Products with associated Category and CoverType objects
         */
        [HttpGet]
        public IActionResult GetAll()
        {
            // Get all Product objects and return them as Json
            var productList = _context.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }
        

        /*
         * Delete(int id) handles a POST request to delete Product with Id of id
         */
        [HttpDelete]
        public IActionResult Delete(int id)
        {

            // Get the Product object
            var obj = _context.Product.GetFirstOrDefault(m => m.Id == id, "");

            // Delete the image
            string wwwPath = _hostEnvironment.WebRootPath;
            var oldImagePath = Path.Combine(wwwPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            // Remove the object from the Db
            _context.Product.Remove(obj);
            _context.Save();

            // Return Json with a success message
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }



}
