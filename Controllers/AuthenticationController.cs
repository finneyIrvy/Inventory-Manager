using Inventory_Management_System__Miracle_Shop_.Models;
using Inventory_Management_System__Miracle_Shop_.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Inventory_Management_System__Miracle_Shop_.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UserManager<NewUserClass> _userManager;
        private readonly SignInManager<NewUserClass> _signInManager;

        public AuthenticationController(UserManager<NewUserClass> userManager, SignInManager<NewUserClass> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: SignUp
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: SignUp
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new NewUserClass
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                City = model.City,
                Gender = model.Gender
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Sign in the user after successful registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Authentication", "Login");
            }

            // Log and display errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Authentication");
        }
    }
}
