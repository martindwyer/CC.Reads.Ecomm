/*
Filename:  ShoppingCartViewComponent.cs
Purpose:   To set up shopping cart display  
Contains:  Business logic for adding and subtracting cart items to the Http session
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CC.Reads.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var cartCount = HttpContext.Session.GetInt32(StaticConstants.SessionCart);
                if (cartCount != null)
                {
                    return View(HttpContext.Session.GetInt32(StaticConstants.SessionCart));
                }
                else
                {
                    cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList()
                        .Count;
                    HttpContext.Session.SetInt32(StaticConstants.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList()
                        .Count);
                    return View(HttpContext.Session.GetInt32(StaticConstants.SessionCart));

                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View();
            }
        }
    }
}
