/*
Filename:  Company.cs
Purpose:   Provide a structure for Company objects  
Contains:  Class properties and data annotations to implement business requirements  
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using Microsoft.Build.Framework;

namespace CC.Reads.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? StreetAddress { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? PostalCode { get; set; }

        public string? PhoneNumber { get; set; }

    }
}
