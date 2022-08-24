/*
Filename:  OrderController.cs
Purpose:   Control requests related to Order objects
Contains:  GET and POST actions methods to create, retrieve, update and delete orders
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-22
By:        Martin Dwyer
*/

using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Models;
using CC.Reads.Models.ViewModel;
using CC.Reads.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using Stripe;

namespace CC.Reads.Areas.Admin.Controllers
{
    [Area("Admin")] // Specifying the area for routing 
    [Authorize] // Authorization is required to work with orders
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork; // Adding Db context  

        [BindProperty] public OrderViewModel Ovm { get; set; } // Binding a view model


        /**
         * Constructor requires a live ApplicationDbContext from the repository as well
         * as a new instance of OrderViewModel
        */
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            Ovm = new OrderViewModel();
        }

        /**
         * Action method to GET all Order objects.  Takes in no parameters.  Data is retrieved
         * and placed into data tables on with JavaScript in the View.
         * 
         * Returns a View with JavaScript to retrieve and display orders.
        */
        public IActionResult Index()
        {
            return View();
        }


        /**
         * Details(int id) action method handles a GET request for Order object details.  The method accepts 
         * an integer, retrieves the OrderHeader and OrderDetails objects from the Db.  The objects are
         * then added to the OrderViewModel, which is returned with Details View.
         * 
         * param id the unique identifier of the OrderHeader object
         * returns a View with the OrderViewModel object        
         */
        public IActionResult Details(int id)
        {
            // Retrieve objects from Db and add them to the OrderViewModel
            Ovm = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == id, "Product"),
            };

            //  Return View with OrderViewModel
            return View(Ovm);
        }

        /**
         * DetailsMakePayment(int id) handles payments on existing orders.  The method accepts an int, id, which
         * is the id for the OrderHeader which contains the Order details.  A Stripe API session is created, with
         * the user being forwarded to a success or cancel page by Stripe as Appropriate.
         * 
         */
        [ActionName("Details")]
        [HttpPost]
        public IActionResult DetailsMakePayment(int id)
        {
            // Retrieve the OrderHeader and OrderDetails objects
            Ovm.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, "ApplicationUser");
            Ovm.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == id, "Product");

            // Setup the root domain for the app
            var domain = "https://cc-reads-app-jmupf.ondigitalocean.app";
            

            // Stripe API options.  Note that LineItems begins here as an empty list
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"/admin/order/PaymentConfirmation?id={Ovm.OrderHeader.Id}",
                CancelUrl = domain + $"/admin/order/details?id={Ovm.OrderHeader.Id}",
            };

            // Add to the LineItems list as appropriate with OrderDetails
            foreach (var item in Ovm.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
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

            // Establish a new Stripe API SessionService
            var service = new SessionService();

            // Use the SessionService to create a Session object
            Session session = service.Create(options);

            // Update the OrderHeader with Session Id and Session PaymentIntentId (if provided)
            // If PaymentIntentId returns null it will be updated on payment confirmation (See PaymentConfirmation)
            _unitOfWork.OrderHeader.UpdateStripePaymentId(Ovm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            // Hand off to Stripe API session url
            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }

        /**
         * PaymentConfirmation(int id) handles the GET request on the return url for a successful payment from
         * Stripe API.  The Stripe API also provides the order id on the url, which is used to retrieve the OrderHeader
         * object from the Db.  The session id (on the OrderHeader) is used to retrieve the session from Stripe.
         * Once the session is retrieved, the paymentIntentId can be retrieved and updated in the Db.
         *  
         */
        public IActionResult PaymentConfirmation(int id)
        {
            // Get the OrderHeader object with the id provided
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            // Create a new Stripe API session service
            var service = new SessionService();

            // Create a session object by retrieving the session from Stripe for this OrderHeader
            Session session = service.Get(orderHeader.SessionId);

            // Update PaymentIntentId and PaymentStatus if paid
            if (session.PaymentStatus.ToLower() == "paid")
            {
                var paymentIntentId = session.PaymentIntentId;
                if (paymentIntentId != null)
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                }

                _unitOfWork.OrderHeader.UpdateStatus(id, orderHeader.OrderStatus,
                    StaticConstants.PaymentStatusApproved);
                _unitOfWork.Save();
            }

            // Return to View with OrderHeader id
            return View(id);
        }

        /**
         * UpdateOrderDetails() processes a POST request to update the OrderHeader properties of an Order.  Details
         * are updated and the user is returned to the Order Details View with the Order id.
         * 
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticConstants.RoleAdmin + "," + StaticConstants.RoleEmployee)]
        public IActionResult UpdateOrderDetails()
        {
            // Order Id taken from OrderViewModel object to retrieve OrderHeader from Db
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == Ovm.OrderHeader.Id);

            // OrderHeader object is updated with OrderViewModel values from Details View
            orderHeaderFromDb.Name = Ovm.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = Ovm.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = Ovm.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = Ovm.OrderHeader.City;
            orderHeaderFromDb.PostalCode = Ovm.OrderHeader.PostalCode;
            orderHeaderFromDb.State = Ovm.OrderHeader.State;
            orderHeaderFromDb.ShippingDate = Ovm.OrderHeader.ShippingDate;
            orderHeaderFromDb.PaymentDate = Ovm.OrderHeader.PaymentDueDate;
            if (Ovm.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = Ovm.OrderHeader.Carrier;
            }

            if (Ovm.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = Ovm.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            // Send a message back for the user
            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction("Details", "Order", new { id = orderHeaderFromDb.Id });
        }


        /**
         * ShipOrder() updates shipping carrier and tracking id on the OrderHeader object before returning the
         * user to the order details page.
         * 
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticConstants.RoleAdmin + "," + StaticConstants.RoleEmployee)]
        public IActionResult ShipOrder()
        {
            // Retrieve the order from the Db
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == Ovm.OrderHeader.Id);

            // Updated shipping and order status 
            orderHeaderFromDb.TrackingNumber = Ovm.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = Ovm.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = Ovm.OrderHeader.OrderStatus;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            // For company orders set due date for payment in 30 days
            if (orderHeaderFromDb.PaymentStatus == StaticConstants.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            // Update OrderHeader in Db
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.OrderHeader.UpdateStatus(Ovm.OrderHeader.Id, StaticConstants.StatusShipped);
            _unitOfWork.Save();

            // Send a message back for the user
            TempData["Success"] = "Order Shipped Successfully";

            return RedirectToAction("Details", "Order", new { id = orderHeaderFromDb.Id });
        }


        /**
         * StartProcessing() updates the OrderStatus to InProcess
         * 
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticConstants.RoleAdmin + "," + StaticConstants.RoleEmployee)]
        public IActionResult StartProcessing()
        {
            // Get the OrderHeader from the Db
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == Ovm.OrderHeader.Id);

            // Updated the OrderHeader OrderStatus
            _unitOfWork.OrderHeader.UpdateStatus(Ovm.OrderHeader.Id, StaticConstants.StatusInProcess);
            _unitOfWork.Save();

            // Send a message back to the user
            TempData["Success"] = "Order Status Updated Successfully";

            return RedirectToAction("Details", "Order", new { id = orderHeaderFromDb.Id });
        }

        /**
         * CancelOrder() updates the OrderStatus and returns the user to the order details page
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticConstants.RoleAdmin + "," + StaticConstants.RoleEmployee)]
        public IActionResult CancelOrder()
        {
            // Retrieve the OrderHeader from the Db
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == Ovm.OrderHeader.Id);
            
            // Refund payment if made
            if (orderHeaderFromDb.PaymentStatus == StaticConstants.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, StaticConstants.StatusCancelled, StaticConstants.StatusRefunded);
            }
            else
            {
                // Update the OrderStatus to cancelled
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, StaticConstants.StatusCancelled,
                    StaticConstants.StatusCancelled);

            }
            
            // Save changes to OrderHeader
            _unitOfWork.Save();

            // Send a message back to the user
            TempData["Success"] = "Order Canceled Successfully";

            return RedirectToAction("Details", "Order", new { id = orderHeaderFromDb.Id });
        }

        /*
         *  The API Calls region handles REST API calls for getting all Orders.  This is used for the Index View  
         */
        #region API CALLS

        /**
         * GetAll(string? status) receives an optional string status indicating the type of orders to be returned.
         * Either all orders or filtered orders are returned based on the status parameter.
         * 
         */
        [HttpGet]
        public IActionResult GetAll(string? status)
        {
            // Create a list to hold the OrderHeader objects
            IEnumerable<OrderHeader> orderHeaders = null;

            // Get information about the current user
            var isAdmin = User.IsInRole(StaticConstants.RoleAdmin);
            var isEmployee = User.IsInRole(StaticConstants.RoleEmployee);

            // If user is Admin or Employee get a list of all orders
            if (User.IsInRole(StaticConstants.RoleAdmin) || User.IsInRole(StaticConstants.RoleEmployee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(filter: null, includeProperties: "ApplicationUser");
            }
            else
            {
                // Retrieve only those orders associated with the current user
                if (User.Identity != null)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    orderHeaders = _unitOfWork.OrderHeader.GetAll(filter: u => u.ApplicationUserId == claim.Value,
                        includeProperties: "ApplicationUser");
                }
            }

            // Filter retrieved orders as indicated in the status parameter
            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticConstants.StatusPending);
                    break;
                
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticConstants.StatusInProcess);
                    break;
                
                case "shipped":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticConstants.StatusShipped);
                    break;
                
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}