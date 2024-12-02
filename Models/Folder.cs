using System;
using System.Collections.Generic;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class Folder
    {
        public int FolderID { get; set; }

        public string FolderName { get; set; }

        // Foreign key to associate the folder with a user
        public string UserID { get; set; }

        // Navigation property to the user who created the folder
        public NewUserClass User { get; set; }

        // Navigation property to the products in the folder
        public ICollection<Product> Products { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}

