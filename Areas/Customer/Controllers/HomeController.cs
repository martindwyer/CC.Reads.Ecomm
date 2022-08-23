/*
Filename:   HomeController.cs
Purpose:    Control requests related to Home page and customer shopping experience
Contains:   GET and POST actions methods to create, retrieve, update and delete ShoppingCart objects
            Ability to process payments and create Order objects
Author:     Martin Dwyer
Created:    2022-08-17
Last Edit:  2022-08-19
By:         Martin Dwyer
*/
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models;
using CC.Reads.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace CC.Reads.Areas.Customer.Controllers

{
    [Area("Customer")]  // Customer area specified for routing purposes
    public class HomeController : Controller
    {

        // Prop _unitOfWork, an instance of UnitofWork repository interface
        private readonly IUnitOfWork _unitOfWork;


        /**
         * Constructor.  Receives an implementation of the data interface.
         * 
         * param unitOfWork an implementation of the UnitOfWork repository interface
         */
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /*
         * Landing method for the site home page preparation returns the correponding view which 
         * can be found in /Areas/Customer/Home/Landing.cshtml 
         * 
         */
        public IActionResult Landing()
        {
            return View();
        }


        /*
         * Index action metho takes in three optional parameters and returns a list of 
         * products to be displayed in the company store.
         * 
         * param searchTerm, a string used to search products
         * param sortBy, a string used to indicate the sort priority
         * param filterBy, a string used to filter the collection of products
         * returns productList, an IEnumerable list of Product objects
         */
        public IActionResult Index(string? searchTerm, string? sortBy, string? filterBy)
        {
            // Create the product list to be returned
            IEnumerable<Product> productList = null;

            // If a search term exists, perform search
            if (searchTerm != null)
            {
                // Get all products which contain the search term in the title, description or author name
                productList = _unitOfWork.Product.GetAll(u => u.Title.Contains(searchTerm) ||
                                                                u.Description.Contains(searchTerm) ||
                                                                u.Author.Contains(searchTerm), includeProperties: "Category,CoverType"
                                                                );

                // Set temp data for display of search term
                TempData["searchTerm"] = searchTerm;
            }
            else
            {
                // With no search term just get all products
                productList = _unitOfWork.Product.GetAll(null, "Category,CoverType");
            }

            //  If a sort term is provided, sort the product list by the sort term
            if (sortBy != null)
            {
                switch (sortBy)
                {
                    case "Price100":
                        productList = productList.OrderBy(u => u.Price100);
                        TempData["sortedBy"] = "Price";
                        break;

                    case "AuthorName":
                        productList = productList.OrderBy(u => u.Author);
                        TempData["sortedBy"] = "Author Name";
                        break;

                    case "CoverType":
                        productList = productList.OrderBy(u => u.CoverType.Name);
                        TempData["sortedBy"] = "Cover Type";
                        break;

                    default:
                        break;

                }

            }

            // If a filter term is provided, filter the collection 
            if (filterBy != null)
            {
                productList = productList.Where(u => u.Category.Name == filterBy);

                TempData["filteredBy"] = filterBy;
                TempData["sortedBy"] = null;
            }


            // Return the sorted and/or filtered product list
            return View(productList);
        }


        /**
         * Details(productId) action method provides the details for a product placed in the shopping cart
         * 
         * 
         * param productId, an integer, indicates the unique identifier for the product
         * returns a ShoppingCart object with only the Product searched for  
         *
         */
        public IActionResult Details(int productId)
        {
            // While retrieving the product by it's Id, create a new ShoppingCart object
            ShoppingCart cart = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, "Category,CoverType")


            };

            // return the cart
            return View(cart);
        }


        /**
         * Details(ShoppincCart) takes in a shopping cart object and either;
         * (a) creates a new shopping cart object and adds it to the database, or
         * (b) adds a shopping cart object to the database
         * 
         * param shoppingCart, a ShoppingCart object which contains a user request 
         * returns the User back to the main store page
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // Establish the User identity and unique identifier
            if (User.Identity != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                // Set the ApplicationUserId property of the shoppingCart object to the User's Id
                shoppingCart.ApplicationUserId = claim.Value;

                // See if an existing cart exists for this User Id and this Product Id
                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u =>
                    u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

                // If no cart exists, add this new cart to the shopping cart data table
                // Add an item to the session cart
                if (cartFromDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                    _unitOfWork.Save();
                    HttpContext.Session.SetInt32(StaticConstants.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
                }
                else
                {
                    // Cart exists, so update the shopping cart with the count being requested
                    _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, (int)shoppingCart.Count);
                    _unitOfWork.Save();
                }

                // Redirect to main store page
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Redirect to main store page
                return RedirectToAction(nameof(Index));
            }


        }


        /**
         * Privacy action method forwards the user to the privacy statement page.
         */
        public IActionResult Privacy()
        {
            return View();
        }


        /**
         * Error action method handles any error and presents the Error view with Current user 
         * Id and HttpContext (if available)
         */
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}