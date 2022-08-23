/*
Filename:  ShoppingCartViewModel.cs
Purpose:   Provide working data structure for ShoppingCart views  
Contains:  Provides and OrderHeader and a list of shopping cart items  
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

namespace CC.Reads.Models.ViewModel
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }

        public OrderHeader OrderHeader { get; set; }

    }
}
