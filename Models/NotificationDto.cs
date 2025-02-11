namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class NotificationDto
    {
        public string Message { get; set; }
        public string Type { get; set; } // "LowStock" or "Movement"
        public int UserId { get; set; } // If notifications are user-specific
    }
}
