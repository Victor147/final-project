﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final_project.Data.Entities
{
    public class Product : Entity<int>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        [Required(ErrorMessage = "Manufacturer cannot be empty!")]
        public Manufacturer Manufacturer { get; set; }
        
        [ForeignKey("Manufacturer")]
        public int ManufacturerId { get; set; }

        [Required(ErrorMessage = "Category cannot be empty")]
        public Category Category { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        
        [NotMapped]
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
