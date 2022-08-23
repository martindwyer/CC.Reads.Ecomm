/*
Filename:  StaticConstants.cs
Purpose:   To eliminate 'magic strings' from the application  
Contains:  Constant definitions which can be used throughout the app
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

namespace CC.Reads.Utility
{
    public class StaticConstants
    {
        public const string RoleUserIndividual = "Individual";
        public const string RoleUserCompany = "Company";
        public const string RoleAdmin = "Admin";
        public const string RoleEmployee = "Employee";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";

        public const string SessionCart = "Shopping Cart";
    }
}
