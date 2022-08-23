/*
Filename:  OrderViewModel.cs
Purpose:   Provide working data structure for Order views  
Contains:  Combines OrderHeader and OrderDetails objects in one model 
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

namespace CC.Reads.Models.ViewModel
{
    public class OrderViewModel
    {
        public OrderHeader OrderHeader { get; set; }

        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
