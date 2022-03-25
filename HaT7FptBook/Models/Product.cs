using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class Product
    {
        [Key] public int Id { get; set; }
        
        [Required] public int CategoryId { get; set; }
        [ForeignKey("CategoryId")] public Category Category { get; set; }
        
        [Required] public string Description { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Author { get; set; }
        [Required] public string ImageUrl { get; set; }
        [Required] public double Price { get; set; }
        [Required] public int NoPage { get; set; }
        public DateTime CreateAt { get; set; }

        public Product()
        {
            CreateAt = DateTime.Now;
        }
    }
}