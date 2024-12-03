using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System__Miracle_Shop_.ViewModel
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please enter your first name.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        public string LastName { get; set; }

        public string City { get; set; }

        public string Gender { get; set; }
    }
}

