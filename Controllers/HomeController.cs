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
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc.Filters;
using Inventory_Management_System__Miracle_Shop_.Views.Home;
using CsvHelper;
using ExcelDataReader;
using System.Globalization;
using System.Data;


namespace Inventory_Management_System__Miracle_Shop_.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<NewUserClass> _userManager;
        private readonly SignInManager<NewUserClass> _signInManager;
        private readonly MiracleDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;



        // Constructor with dependency injection
        public HomeController(ILogger<HomeController> logger,
                              IHubContext<NotificationHub> hubContext,
                              UserManager<NewUserClass> userManager,
                              SignInManager<NewUserClass> signInManager,
                              MiracleDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _hubContext = hubContext;

        }
        public async Task<IActionResult> Index(string searchTerm)
        {
            var userId = _userManager.GetUserId(User);

            var foldersQuery = _context.Folders
                                       .Where(f => f.UserID == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                foldersQuery = foldersQuery.Where(f => f.FolderName.Contains(searchTerm));
            }

            var folders = await foldersQuery.ToListAsync();

            return View(folders);
        }

        [HttpGet]
        public IActionResult Purchase(int id)
        {
            // Process the purchase logic
            return View();
        }


       

        [HttpGet("notifications")]
        public async Task<IActionResult> NotificationView()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("NotificationView", notifications);
        }

        [HttpPost("add-notification")]
        public async Task<IActionResult> AddNotification([FromBody] NotificationDto notificationDto)
        {
            var notification = new Notification
            {
                Message = notificationDto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = notificationDto.Type, // "LowStock" or "Movement"
                UserId = notificationDto.UserId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification using SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Message);

            return Ok(notification);
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                // Fetch low stock count
                int lowStockCount = _context.Products
                    .Where(p => p.UserID == userId && p.Quantity <= p.MinStockLevel)
                    .Count();

                ViewBag.LowStockCount = lowStockCount;
            }

            base.OnActionExecuting(context);
        }



        [HttpGet]
        public async Task<IActionResult> Products(string searchTerm)
        {
            // Get the current logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // Retrieve the products associated with the logged-in user
            var productsQuery = _context.Products.Where(p => p.UserID == userId);

            // Apply search filter if a search term is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchTerm) ||
                                                         p.Category.Contains(searchTerm) ||
                                                         p.Description.Contains(searchTerm) ||
                                                         p.Location.Contains(searchTerm));
            }

            var products = await productsQuery.ToListAsync();

            // Filter low stock products
            var lowStockProducts = products.Where(p => p.Quantity <= p.MinStockLevel).ToList();

            // Save low stock notifications
            if (lowStockProducts.Any())
            {
                foreach (var product in lowStockProducts)
                {
                    // Check if a notification for this product already exists
                    var existingNotification = await _context.Notifications
                        .FirstOrDefaultAsync(n => n.Message.Contains(product.ProductName) && n.Type == "LowStock");

                    if (existingNotification == null) // Avoid duplicate notifications
                    {
                        var notification = new Notification
                        {
                            Message = $"Low stock alert: {product.ProductName} has only {product.Quantity} left!",
                            Type = "LowStock",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();

                        // Send notification via SignalR
                        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Message);
                    }
                }
            }

            // Store low stock count in ViewBag
            ViewBag.LowStockCount = lowStockProducts.Count;

            return View(products);
        }




        // POST: Handle the move product form submission
        public async Task<IActionResult> MoveProduct(int id, int destinationFolderId)
        {
            var userId = _userManager.GetUserId(User);
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductID == id && p.UserID == userId);

            if (product == null)
            {
                TempData["Message"] = "Product not found or you do not have access to this product.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var destinationFolder = await _context.Folders
                .FirstOrDefaultAsync(f => f.FolderID == destinationFolderId && f.UserID == userId);

            if (destinationFolder == null)
            {
                TempData["Message"] = "Destination folder not found or you do not have access to this folder.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var previousFolderId = product.FolderID;
            product.FolderID = destinationFolderId;

            // Log the stock movement
            var stockMovement = new StockMovement
            {
                ProductID = product.ProductID,
                MovementType = "Moved",
                QuantityChanged = 0,
                SourceLocation = previousFolderId.ToString(),
                DestinationLocation = destinationFolderId.ToString(),
                MovementDate = DateTime.Now
            };

            _context.StockMovement.Add(stockMovement);

            // Save notification in the database
            var notificationMessage = $"{product.ProductName} moved from Folder {previousFolderId} to {destinationFolder.FolderName}.";

            var notification = new Notification
            {
                Message = notificationMessage,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send notification using SignalR
            Console.WriteLine("Sending SignalR Notification: " + notificationMessage);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationMessage);

            TempData["Message"] = $"{product.ProductName} moved to {destinationFolder.FolderName} successfully!";
            TempData["MessageType"] = "success";

            return RedirectToAction("ViewFolder", new { folderId = destinationFolderId });
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
            if (folderId <= 0)
            {
                TempData["Message"] = "Invalid folder ID.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (!_signInManager.IsSignedIn(User))
            {
                TempData["Message"] = "You need to log in first.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            var userFolders = await _context.Folders
                                            .Where(f => f.UserID == user.Id)
                                            .Include(f => f.Products)
                                            .ToListAsync();

            var selectedFolder = userFolders.FirstOrDefault(f => f.FolderID == folderId);

            if (selectedFolder == null)
            {
                TempData["Message"] = "Folder not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            ViewBag.Folder = selectedFolder.FolderName;
            ViewBag.Products = selectedFolder.Products?.ToList() ?? new List<Product>();
            ViewBag.AllFolders = userFolders;
            ViewBag.FolderID = folderId; // ✅ Pass the correct folder ID to the view

            return View(selectedFolder);
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file, int folderId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "No file uploaded or file is empty.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ViewFolder", new { folderId });
            }

            // Get logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login", "Account");
            }

            // Check if the folder belongs to the user
            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.FolderID == folderId && f.UserID == user.Id);

            if (folder == null)
            {
                TempData["Message"] = "Invalid folder selection.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true // Ensure the first row is treated as column headers
                            }
                        });

                        var dataTable = result.Tables[0];
                        var products = new List<Product>();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            products.Add(new Product
                            {
                                ProductName = row["ProductName"].ToString(),
                                Category = row["Category"].ToString(),
                                Location = row["Location"].ToString(),
                                Type = row["Type"].ToString(),
                                Description = row["Description"].ToString(),
                                Quantity = int.TryParse(row["Quantity"].ToString(), out int qty) ? qty : 0,
                                Cost = decimal.TryParse(row["Cost"].ToString(), out decimal cost) ? cost : 0,
                                UserID = user.Id,  // Auto-assign logged-in user
                                FolderID = folderId // Auto-assign selected folder
                            });
                        }

                        if (products.Any())
                        {
                            _context.Products.AddRange(products);
                            await _context.SaveChangesAsync();
                            TempData["Message"] = "Products imported successfully!";
                            TempData["MessageType"] = "success";
                        }
                        else
                        {
                            TempData["Message"] = "No valid products found in the file.";
                            TempData["MessageType"] = "warning";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error processing the file: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("ViewFolder", new { folderId });
        }




        public IActionResult ExportToPDF(int folderId)
        {
            var products = _context.Products.Where(p => p.FolderID == folderId).ToList();

            if (!products.Any())
            {
                TempData["Message"] = "No products found in the selected folder.";
                TempData["MessageType"] = "warning"; // Optional: Set a type for styling
                return RedirectToAction("ViewFolder", new { folderId });
            }


            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                Paragraph title = new Paragraph("Product List - Folder " + folderId, titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                document.Add(title);

                // Create Table
                PdfPTable table = new PdfPTable(5); // 5 columns
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2f, 2f, 1.5f, 2f });

                // Table Headers
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                string[] headers = { "Product Name", "Category", "Location", "Quantity", "Cost" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(211, 211, 211),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Table Data
                Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var product in products)
                {
                    table.AddCell(new PdfPCell(new Phrase(product.ProductName, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(product.Category, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(product.Location, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(product.Quantity.ToString(), cellFont)));
                    table.AddCell(new PdfPCell(new Phrase("$" + product.Cost.ToString("F2"), cellFont)));
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Products_Folder_{folderId}.pdf");
            }
        }

        public async Task<IActionResult> StockMovements()
        {
            var movements = await _context.StockMovement
                                          .Include(m => m.Product) // Ensure Product details are included
                                          .OrderByDescending(m => m.MovementDate)
                                          .ToListAsync();

            return View(movements);
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

            // ✅ Check stock levels and trigger notification
            if (product.Quantity <= product.MinStockLevel)
            {
                string statusMessage = $"⚠️ Low stock alert: {product.ProductName} has only {product.Quantity} left!";

                // Check if a similar notification already exists
                var existingNotification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Message.Contains(product.ProductName) && n.Type == "LowStock");

                if (existingNotification == null)
                {
                    var notification = new Notification
                    {
                        Message = statusMessage,
                        Type = "LowStock",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // 🔔 Send SignalR notification
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", statusMessage);
                }
            }

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
