using System.Collections.Generic;
using System.Linq;
using Inventory_Management_System__Miracle_Shop_.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System__Miracle_Shop_.Controllers
{
    [Route("cart")]
    public class CartController : Controller
    {
        private const string CartSessionKey = "CartItems";

        
        [HttpPost("add")]
        public IActionResult AddToCart([FromBody] CartItem cartItem)
        {
            if (cartItem == null)
            {
                return BadRequest(new { success = false, message = "Invalid product data." });
            }

            if (cartItem.ProductID <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid Product ID." });
            }

            // Retrieve cart from session
            List<CartItem> cart = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Check if item is already in cart
            var existingItem = cart.FirstOrDefault(p => p.ProductID == cartItem.ProductID);
            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
            }
            else
            {
                cart.Add(cartItem);
            }

            // Save cart back to session
            HttpContext.Session.SetObject("CartItems", cart);

            return Json(new { success = true, message = "Product added to cart." });
        }

        [HttpGet("count")]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();
            int totalItems = cart.Sum(item => item.Quantity);

            return Json(new { count = totalItems });
        }


        [HttpGet("view")]
        public IActionResult ViewCart()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();
            return View("ViewCart", cart); // Ensure "CartView.cshtml" exists
        }


        [HttpPost("clear")]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return Json(new { success = true, message = "Cart cleared." });
        }
    }
}
