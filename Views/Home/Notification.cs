using System;

namespace Inventory_Management_System__Miracle_Shop_.Views.Home
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } // "LowStock" or "Movement"
        public int UserId { get; set; } // If notifications are user-specif
    }


}
