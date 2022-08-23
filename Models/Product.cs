/*
Filename:  Product.cs
Purpose:   Provide a structure for Product objects  
Contains:  Class properties and data annotations to implement business requirements  
           Note the foreign key specifications to CoverType and Category
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/


using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CC.Reads.Models
{
    public class Product
    {

        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }


        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(0, 10000)]
        [DisplayName("List Price")]
        public double ListPrice { get; set; }

        [Required]
        [Range(0, 10000)]
        [DisplayName("Price for up to 50 copies")]
        public double Price { get; set; }

        [Required]
        [Range(0, 10000)]
        [DisplayName("Price for 50 to 100 copies")]
        public double Price50 { get; set; }

        [Required]
        [Range(0, 10000)]
        [DisplayName("Price for over 100 copies")]
        public double Price100 { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }

        [DisplayName("Category")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }

        [DisplayName("Cover Type")]
        public int? CoverTypeId { get; set; }

        [ForeignKey("CoverTypeId")]
        [ValidateNever]
        public CoverType? CoverType { get; set; }


        public Product()
        {
            Title = "";
            ISBN = "";
            Description = "";
            Author = "";
            ImageUrl = "";
            ListPrice = 0.00;
            Price = 0.00;
            Price50 = 0.00;
            Price100 = 0.00;

        }


    }
}
