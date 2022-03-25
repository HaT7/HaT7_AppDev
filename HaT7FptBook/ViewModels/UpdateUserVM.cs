using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HaT7FptBook.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaT7FptBook.ViewModels
{
    public class UpdateUserVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        [Required] public string Role { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}