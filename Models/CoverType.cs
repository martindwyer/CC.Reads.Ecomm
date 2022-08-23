/*
Filename:  CoverType.cs
Purpose:   Provide a structure for CoverType objects  
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
    public class CoverType
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Cover Type")]
        [Required]
        public string Name { get; set; }

        public CoverType()
        {
            Name = "";
        }
    }
}
