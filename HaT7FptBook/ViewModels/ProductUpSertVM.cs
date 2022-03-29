using System.Collections.Generic;
using HaT7FptBook.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaT7FptBook.ViewModels
{
    public class ProductUpSertVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}