using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class NewUserClass : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public string City { get; set; }

        public string Gender { get; set; }

        public DateTime? LastLoginTime { get; set; }
    }
}
