namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }  // Must match JSON property "productId"
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; } = 1;
    }

}
