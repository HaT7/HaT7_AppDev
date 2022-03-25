using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class Cart
    {
        [Key] public int Id { get; set; }
        
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")] public ApplicationUser ApplicationUser { get; set; }
        
        [Required] public int ProductId { get; set; }
        [ForeignKey("ProductId")] public Product Product { get; set; }
        
        [Required] public int Count { get; set; }

        public DateTime CreateAt { get; set; }

        public Cart()
        {
            CreateAt = DateTime.Now;
        }
    }
}