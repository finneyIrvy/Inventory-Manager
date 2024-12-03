using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class Product
    {
        // Unique identifier for the product
        public int ProductID { get; set; }

        // Name of the product
        [Required(ErrorMessage = "Please enter the product name.")]
        public string ProductName { get; set; }

        // Category to which the product belongs
        [Required(ErrorMessage = "Please enter the category.")]
        public string Category { get; set; }

        // Location of the product (e.g., warehouse, room)
        public string Location { get; set; }

        // Type of the product (e.g., equipment, consumable)
        public string Type { get; set; }

        // Description of the product
        public string Description { get; set; }

        // Path or URL of the uploaded product image
        public string ImagePath { get; set; }

        // Foreign key to associate the product with a user
        public string UserID { get; set; }

        // Navigation property to the user who created the product
        [ForeignKey("UserID")]
        public NewUserClass User { get; set; }

        // Foreign key to associate the product with a folder
        public int? FolderID { get; set; }

        // Navigation property to the folder that contains the product
        [ForeignKey("FolderID")]
        public Folder Folder { get; set; }

        // Quantity of the product in stock
        [Required(ErrorMessage = "Please enter the quantity.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative value.")]
        public int Quantity { get; set; }

        // Cost of the product
        
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be a positive value.")]
        public decimal Cost { get; set; }

        // Date when the product was added
        [Required(ErrorMessage = "Please enter the date of addition.")]
        public DateTime DateAdded { get; set; } = DateTime.Now; // Default to current date
    }
}
