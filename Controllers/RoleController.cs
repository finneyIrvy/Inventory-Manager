using Inventory_Management_System__Miracle_Shop_.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RoleController : Controller
{

    private readonly UserManager<NewUserClass> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<NewUserClass> _signInManager;

    public RoleController(UserManager<NewUserClass> userManager, SignInManager<NewUserClass> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }


  
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();

        // Create a dictionary to store user roles
        var userRoles = new Dictionary<string, List<string>>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user); // Get roles of each user
            userRoles[user.Id] = roles.ToList();
        }

        // Pass the userRoles dictionary to the view through ViewBag
        ViewBag.UserRoles = userRoles;

        return View(users);
    }


    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            ViewBag.Message = "User ID is required.";
            return RedirectToAction("Users");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            ViewBag.Message = "User not found!";
            return RedirectToAction("Users");
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(string id, string firstName, string lastName, string email, string userName, string gender)
    {
        if (string.IsNullOrEmpty(id))
        {
            ViewBag.Message = "Invalid user.";
            return RedirectToAction("Users");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            ViewBag.Message = "User not found!";
            return RedirectToAction("Users");
        }
        
        // Update user properties
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.UserName = userName;
        user.Gender = gender;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            ViewBag.Message = "Profile updated successfully!";
        }
        else
        {
            ViewBag.Message = "Error updating user: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return View(user);
    }

  
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["Message"] = "Invalid user.";
            return RedirectToAction("Users");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = "User not found!";
            return RedirectToAction("Users");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            // Include user's full name in the success message
            TempData["Message"] = $"{user.FirstName} {user.LastName}'s profile deleted successfully!";
        }
        else
        {
            TempData["Message"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction("Users");
    }



    // Action to render the role creation view
    public IActionResult CreateRole()
    {
        return View();
    }

    // Action to handle form submission and create the role
    [HttpPost]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            ViewBag.Message = "Role name cannot be empty.";
            return View();
        }

        var roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                ViewBag.Message = $"Role '{roleName}' created successfully.";
                return View();
            }

            ViewBag.Message = "Error in creating role.";
            return View();
        }

        ViewBag.Message = "Role already exists.";
        return View();
    }

    // Action to view all roles
    public async Task<IActionResult>Roles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return View(roles); // Pass roles to the view
    }

    // Action to render the edit view for a specific role
    // Action to render the edit view for a specific role
    public async Task<IActionResult> EditRole(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return View(role); // This should pass a single IdentityRole model to the EditRole view
    }

    
    [HttpPost]
    public async Task<IActionResult> EditRole(string id, string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            ViewBag.Message = "Role name cannot be empty.";
            return View();
        }

        var role = await _roleManager.FindByIdAsync(id);

        if (role == null)
        {
            ViewBag.Message = "Role not found.";
            return View();
        }

        role.Name = roleName;
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            ViewBag.Message = $"Role updated to '{roleName}' successfully.";
            return View ();
        }
        else
        {
            ViewBag.Message = "Error in updating role.";
        }

        return View(role);
    }


    // Action to handle role deletion
    [HttpPost, ActionName("DeleteRoleConfirmed")]
    public async Task<IActionResult> DeleteRoleConfirmed(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            TempData["Message"] = $"Role '{role.Name}' deleted successfully.";
            return RedirectToAction("Roles");
        }

        TempData["Message"] = "Error in deleting role.";
        return RedirectToAction("Roles");
    }

    [HttpGet]
    public async Task<IActionResult> RoleUser()
    {
        // Get all users
        var users = await _userManager.Users.ToListAsync();

        // Get all roles
        var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

        // Pass users and roles to the view
        ViewBag.Roles = roles;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(List<string> selectedUsers, string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            TempData["Message"] = "Please select a role.";
            return RedirectToAction("RoleUser");
        }

        if (selectedUsers == null || !selectedUsers.Any())
        {
            TempData["Message"] = "Please select at least one user.";
            return RedirectToAction("RoleUser");
        }

        foreach (var userId in selectedUsers)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && await _roleManager.RoleExistsAsync(roleName))
            {
                // Get all the current roles of the user
                var userRoles = await _userManager.GetRolesAsync(user);

                // Remove the user from any existing roles
                foreach (var userRole in userRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, userRole);
                }

                // Assign the new role
                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (!result.Succeeded)
                {
                    TempData["Message"] = $"Failed to assign role to user '{user.UserName}'.";
                    return RedirectToAction("RoleUser");
                }
            }
        }

        TempData["Message"] = $"Role '{roleName}' successfully assigned .";
        return RedirectToAction("RoleUser");
    }








}
