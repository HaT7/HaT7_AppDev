using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class Cart
    {
        [Key] public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")] public ApplicationUser ApplicationUser { get; set; }
        public int BookId { get; set; }
        [ForeignKey("BookId")] public Book Book { get; set; }
        [Required] public int Count { get; set; }
        public DateTime CreateAt { get; set; }
        [NotMapped] public double Price { get; set; }
        public Cart()
        {
            CreateAt = DateTime.Now;
            Count = 1;
        }
    }
}