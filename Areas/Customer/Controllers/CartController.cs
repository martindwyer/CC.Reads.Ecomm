/*
Filename:   CartController.cs
Purpose:    Control requests related to ShoppingCart
Contains:   GET and POST actions methods to create, retrieve, update and delete ShoppingCart objects
Author:     Martin Dwyer
Created:    2022-08-17
Last Edit:  2022-08-19
By:         Martin Dwyer
*/
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models;
using CC.Reads.Models.ViewModel;
using CC.Reads.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace CC.Reads.Areas.Customer.Controllers
{
    [Area("Customer")]  // Specified for routing purposes
    [Authorize]                 // Users must be authorized to access
    public class CartController : Controller
    {
        // Prop _unitOfWork, an instance of UnitofWork repository interface
        private readonly IUnitOfWork _unitOfWork;
       
        // View model to be used
        public ShoppingCartViewModel Svm { get; set; }   

        // The total for each Cart object
        public double OrderTotal { get; set; }

        /*
         * Constructor requires an implementation of the IUnitofWork interface
         */
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /*
         * Action method to GET all Cart objects.  Takes in no parameters.  Data is retrieved
         * and placed into data tables on with JavaScript in the View.
         * 
         * Returns a View with JavaScript to retrieve and display orders.
        */
        public IActionResult Index()
        {
            // Get current user information
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // Create a new ShoppingCartViewModel with all carts belonging to the user
            Svm = new ShoppingCartViewModel()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product"),
                OrderHeader = new()
            };
            
            // Add each cart total to the OrderHeader total
            foreach (var cart in Svm.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                    cart.Product.Price100);
                Svm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(Svm);
        }

        /*
         * Summary() provides a summary of the current user's shopping cart items. 
         */
        public IActionResult Summary()
        {
            // Getting current user identity and Id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // Assimilating a ShoppingCart object for the user
            Svm = new ShoppingCartViewModel()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product"),
                OrderHeader = new()
            };
            
            // Get the user object and set user in the OrderHeader
            Svm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            
            // Setting OrderHeader information to match existing Db values for user
            Svm.OrderHeader.Name = Svm.OrderHeader.ApplicationUser.Name;
            Svm.OrderHeader.StreetAddress = Svm.OrderHeader.ApplicationUser.StreetAddress;
            Svm.OrderHeader.City = Svm.OrderHeader.ApplicationUser.City;
            Svm.OrderHeader.State = Svm.OrderHeader.ApplicationUser.State;
            Svm.OrderHeader.PostalCode = Svm.OrderHeader.ApplicationUser.PostalCode;
            Svm.OrderHeader.PhoneNumber = Svm.OrderHeader.ApplicationUser.PhoneNumber;

            // Calculating OrderTotal for the OrderHeader object
            foreach (var cart in Svm.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                    cart.Product.Price100);
                Svm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(Svm);
        }

        /*
         * Summary(ShoppingCart svm) handles a POST request for the user to proceed to payment.  On submitting the
         * request, the user is forwarded to Stripe API for payment processing.
         */
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPOST(ShoppingCartViewModel Svm)
        {
            // Getting the current user Identity and Id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // Construct the view model with list of user's shopping carts
            Svm.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, "Product");

            // Set OrderHeader OrderDate and ApplicationUserId
            Svm.OrderHeader.OrderDate = System.DateTime.Now;
            Svm.OrderHeader.ApplicationUserId = claim.Value;

            // Setting the order total
            foreach (var cart in Svm.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                    cart.Product.Price100);
                Svm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            // Retrieving the application user object
            var applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            // If not a company user then set status for payment to be processed
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                Svm.OrderHeader.PaymentStatus = StaticConstants.PaymentStatusPending;
                Svm.OrderHeader.OrderStatus = StaticConstants.StatusPending;
            }
            else
            {   // Company users have PaymentStatus which is delayed
                Svm.OrderHeader.PaymentStatus = StaticConstants.PaymentStatusDelayedPayment;
                Svm.OrderHeader.OrderStatus = StaticConstants.StatusApproved;
            }

            // Saving the OrderHeader to the Db
            _unitOfWork.OrderHeader.Add(Svm.OrderHeader);
            _unitOfWork.Save();

            // Convert each cart into an OrderDetail object with the OrderHeader Id
            foreach (var cart in Svm.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = Svm.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            // Prepare to process payment for non company users
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Specify the root domain of the website
                var domain = "https://cc-reads-app-jmupf.ondigitalocean.app";

                // Setup options for Stripe API with LineItems as an empty list
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"/customer/cart/OrderConfirmation?id={Svm.OrderHeader.Id}",
                    CancelUrl = domain + $"/customer/cart/index",

                };

                // Populate LineItems with detail from each ShoppingCart
                foreach (var item in Svm.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                // Create a Stripe API session service
                var service = new SessionService();
                
                // Use the session service to create a Stripe API session
                Session session = service.Create(options);

                // Update SessionId and PaymentIntentId in the OrderHeader
                // PaymentIntentId may be null so make sure to check it again in OrderConfirmation below
                _unitOfWork.OrderHeader.UpdateStripePaymentId(Svm.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                
                // Forward to Stripe API session url
                Response.Headers.Add("Location", session.Url);
                
                return new StatusCodeResult(303);
            }
            else
            {
                // For company users who do not have to pay when ordering
                return RedirectToAction("OrderConfirmation", "Cart", new { id = Svm.OrderHeader.Id });
            }
        }

        /*
         * OrderConfirmation(int id) action method handles GET requests to the OrderConfirmation url.  On successful
         * payment, Stripe sends a GET request to this url with an id parameter representing the OrderHeader id.
         * 
         */
        public IActionResult OrderConfirmation(int id)
        {
            // Get the OrderHeader with the id provided
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            // Retrieve the Stripe session if it is not a company order
            if (orderHeader.PaymentStatus != StaticConstants.PaymentStatusDelayedPayment)
            {
                // Create a service with SessionService
                var service = new SessionService();
                
                // Use the service to get the session corresponding to the 
                Session session = service.Get(orderHeader.SessionId);

                // This will make sure we pick up the payment intent id if it was null above
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    var paymentIntentId = session.PaymentIntentId;
                    if (paymentIntentId != null)
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                    }
                    
                    // Update OrderStatus and PaymentStatus
                    _unitOfWork.OrderHeader.UpdateStatus(id, StaticConstants.StatusPending,
                        StaticConstants.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            else
            {
                // Update OrderStatus (PaymentStatus was already set to delayed)
                _unitOfWork.OrderHeader.UpdateStatus(id, StaticConstants.StatusPending);
                _unitOfWork.Save();
            }

            // Remove all the shopping carts for the user from the Db
            List<ShoppingCart> carts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId)
                .ToList();
            _unitOfWork.ShoppingCart.RemoveRange(carts);
            _unitOfWork.Save();
            
            // Clear the Http session context
            HttpContext.Session.Clear();
            
            return View(id);
        }


        /*
         * Plus(int cartId) handles when the user clicks a + button on the Order details view.  The number of items
         * in the cart is incremented
         */
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            
            return RedirectToAction(nameof(Index));
        }

        /*
         * Minus(int cartId) handles when the user clicks a - button on the Order details view.  The number of items
         * in the cart is decremented or, if count = 1, the cart is removed
         */
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);

            // Make sure to remove shopping cart if Count = 1
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                HttpContext.Session.SetInt32(StaticConstants.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        /*
         * Remove(int cartId) removes the cart associated with cartId from the Db
         */
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);

            _unitOfWork.Save();

            HttpContext.Session.SetInt32(StaticConstants.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count);

            return RedirectToAction(nameof(Index));
        }


        /*
         * GetPriceBasedOnQuantity(...) is a helper method to reflect quantity discounts in the price the user
         * will have in their cart
         */
        private double GetPriceBasedOnQuantity(int quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else if (quantity <= 100)
            {
                return price50;
            }
            else
            {
                return price100;
            }
        }
    }
}
