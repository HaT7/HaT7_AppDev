using System.Collections.Generic;
using HaT7FptBook.Models;

namespace HaT7FptBook.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<Cart> ListCarts { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}