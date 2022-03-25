using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HaT7FptBook.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required] public string FullName { get; set; }
        [Required] public string Address { get; set; }
        [NotMapped] public string Role { get; set; }

        public DateTime CreateAt { get; set; }

        public ApplicationUser()
        {
            CreateAt = DateTime.Now;
        }
    }
}