/*
Filename:  OrderViewModel.cs
Purpose:   Provide working data structure for Product views  
Contains:  Combines Product with lists for categories and cover types 
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CC.Reads.Models.ViewModel
{
    public class ProductViewModel
    {
        public Product Product { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }

        public ProductViewModel()
        {
            Product = new Product();
            CoverTypeList = new List<SelectListItem>();
            CategoryList = new List<SelectListItem>();
        }
    }
}
