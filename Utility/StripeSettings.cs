/*
Filename:  StripeSettings.cs
Purpose:   To import the Stripe API keys  
Contains:  Required syntax 
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

namespace CC.Reads.Utility
{
    public class StripeSettings
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }

    }
}
