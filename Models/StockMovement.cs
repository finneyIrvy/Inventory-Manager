using System;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class StockMovement
    {
        public int StockMovementID { get; set; }
        public int ProductID { get; set; }
        public string MovementType { get; set; } // "Added", "Removed", "Transferred"
        public int QuantityChanged { get; set; }
        public string? SourceLocation { get; set; }
        public string? DestinationLocation { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Now;

        // Navigation Property
        public Product Product { get; set; }
    }
}
