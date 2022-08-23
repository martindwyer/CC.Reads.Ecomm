/*
Filename:  Category.cs
Purpose:   Provide a structure for Category objects  
Contains:  Class properties and data annotations to implement business requirements  
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Reads.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100.")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Category()
        {
            Name = "";

        }
    }
}
