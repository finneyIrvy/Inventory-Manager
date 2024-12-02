using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class Product
    {
        // Unique identifier for the product
        public int ProductID { get; set; }

        // Name of the product
        public string ProductName { get; set; }

        // Category to which the product belongs
        public string Category { get; set; }

        // Location of the product (e.g., warehouse, room)
        public string Location { get; set; }

        // Type of the product (e.g., equipment, consumable)
        public string Type { get; set; }

        // Description of the product
        public string Description { get; set; }

        // SKU (Stock Keeping Unit)
        public string SKU { get; set; }

        // Path or URL of the uploaded product image
        public string ImagePath { get; set; }

        // Foreign key to associate the product with a user
        public string UserID { get; set; } // Change to string (matching NewUserClass primary key)

        // Navigation property to the user who created the product
        [ForeignKey("UserID")]
        public NewUserClass User { get; set; }

        // Foreign key to associate the product with a folder
        public int? FolderID { get; set; }

        // Navigation property to the folder that contains the product
        [ForeignKey("FolderID")]
        public Folder Folder { get; set; }
    }
}
