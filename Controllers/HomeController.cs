using Inventory_Management_System__Miracle_Shop_.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Inventory_Management_System__Miracle_Shop_.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<NewUserClass> _userManager;
        private readonly SignInManager<NewUserClass> _signInManager;
        private readonly MiracleDbContext _context;

        // Constructor with dependency injection
        public HomeController(ILogger<HomeController> logger,
                              UserManager<NewUserClass> userManager,
                              SignInManager<NewUserClass> signInManager,
                              MiracleDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // Retrieve all folders associated with the user asynchronously
            var folders = await _context.Folders
                                        .Where(f => f.UserID == userId)
                                        .ToListAsync();  // Use ToListAsync for async query

            // Pass the list of folders to the view
            return View(folders);
        }


        public async Task<IActionResult> Products()
        {
            // Get the current logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // Retrieve the products associated with the logged-in user
            var products = await _context.Products
                                          .Where(p => p.UserID == userId) // Filter by user ID
                                          .ToListAsync();

            // Pass the list of products to the view
            return View(products);
        }



        [HttpPost]
        public async Task<IActionResult> CreateFolder(string folderName)
        {
            var userId = _userManager.GetUserId(User); // Get the current logged-in user ID

            // Check if the folder already exists for this user
            var existingFolder = await _context.Folders
                                               .FirstOrDefaultAsync(f => f.FolderName == folderName && f.UserID == userId);

            if (existingFolder != null)
            {
                // Folder already exists, set a denial message using TempData
                TempData["Message"] = $"A folder with the name '{folderName}' already exists.";
                TempData["MessageType"] = "error"; // Optional: to specify the message type (error, success)
                return RedirectToAction("Index"); // Redirect to the Index view
            }

            // Create a new folder if it doesn't exist
            var folder = new Folder
            {
                FolderName = folderName,
                UserID = userId
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            // Folder successfully created, set a success message using TempData
            TempData["Message"] = $"Folder '{folderName}' successfully added!";
            TempData["MessageType"] = "success"; // Optional: to specify the message type
            return RedirectToAction("Index"); // Redirect to the Index view
        }

        [HttpGet]
        public async Task<IActionResult> ViewFolder(int folderId)
        {
            var folder = await _context.Folders
                                       .Include(f => f.Products)
                                       .FirstOrDefaultAsync(f => f.FolderID == folderId);

            if (folder == null)
            {
                TempData["Message"] = "Folder not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(folder);
        }



        [HttpPost]
        public async Task<IActionResult> AddProductToFolder(int folderId, Product product, IFormFile imageFile)
        {
            var userId = _userManager.GetUserId(User);

            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.FolderID == folderId && f.UserID == userId);

            if (folder == null)
            {
                TempData["Message"] = "Folder not found or you do not have access to this folder.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Handle image file upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImagePath = $"/uploads/{uniqueFileName}"; // Save relative path in the database
            }

            product.FolderID = folderId;
            product.UserID = userId;
            if (product.DateAdded == default) product.DateAdded = DateTime.Now;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{product.ProductName} added to folder successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("ViewFolder", new { folderId });
        }





        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product updatedProduct)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please correct the errors.";
                TempData["MessageType"] = "error";
                return View(updatedProduct);
            }

            // Check if file is uploaded and handle it
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null && file.Length > 0)
            {
                // Define file path
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);

                // Save the file to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update the ImagePath property with the file path
                updatedProduct.ImagePath = "/images/" + file.FileName;
            }
            else if (string.IsNullOrEmpty(updatedProduct.ImagePath))
            {
                // Ensure that ImagePath is not left empty if no file is uploaded
                TempData["Message"] = "Please upload a product image.";
                TempData["MessageType"] = "error";
                return View(updatedProduct);
            }

            // Find the product to update
            var product = await _context.Products.FindAsync(updatedProduct.ProductID);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Update the product details
            product.ProductName = updatedProduct.ProductName;
            product.Category = updatedProduct.Category;
            product.Description = updatedProduct.Description;
            product.Location = updatedProduct.Location;
            product.ImagePath = updatedProduct.ImagePath; // Updated image path

            // Save changes to the database
              await _context.SaveChangesAsync();

            TempData["Message"] = "Product updated successfully.";
            TempData["MessageType"] = "success";
            return RedirectToAction("ViewFolder", new { folderId = product.FolderID });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Product deleted successfully.";
            TempData["MessageType"] = "success";
            return RedirectToAction("ViewFolder", new { folderId = product.FolderID });
        }


        // GET: Show the edit form for a specific folder
        public async Task<IActionResult> Edit(int id)
        {
            // Retrieve the folder by ID from the database
            var folder = await _context.Folders
                                       .FirstOrDefaultAsync(f => f.FolderID == id);

            // If the folder doesn't exist, return a NotFound result
            if (folder == null)
            {
                return NotFound();
            }

            // Return the folder to the view for editing
            return View(folder);
        }


        // POST: Save the changes to the folder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string folderName)
        {
            // Retrieve the folder by ID
            var folder = await _context.Folders.FindAsync(id);

            // If the folder doesn't exist, return a NotFound result
            if (folder == null)
            {
                return NotFound();
            }

            // Check if the folder name is different
            if (folder.FolderName != folderName)
            {
                folder.FolderName = folderName;
                await _context.SaveChangesAsync();
            }

          
            // Set a success message using TempData with the folder's name
            TempData["Message"] = $"Folder '{folder.FolderName}' Edited successfully!";
            TempData["MessageType"] = "success";


            // Redirect back to the Index page after editing
            return RedirectToAction("Index");
        }

       
        // POST: Delete the folder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve the folder by ID
            var folder = await _context.Folders.FindAsync(id);

            // If the folder doesn't exist, return a NotFound result
            if (folder == null)
            {
                return NotFound();
            }

            // Delete the folder from the database
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();

            // Set a success message using TempData with the folder's name
            TempData["Message"] = $"Folder '{folder.FolderName}' deleted successfully!";
            TempData["MessageType"] = "success";


            // Redirect back to the Index page after deletion
            return RedirectToAction("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
