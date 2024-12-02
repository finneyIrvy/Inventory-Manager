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
                TempData["Message"] = "A folder with this name already exists.";
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
            TempData["Message"] = "Folder successfully Added!";
            TempData["MessageType"] = "success"; // Optional: to specify the message type
            return RedirectToAction("Index"); // Redirect to the Index view
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



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
