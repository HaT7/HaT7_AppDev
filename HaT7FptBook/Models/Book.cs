using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class Book
    {
        [Key] public int Id { get; set; }
        [Required] public int StoreId { get; set; }
        [ForeignKey("StoreId")] public Store Store { get; set; }
        [Required] public int CategoryId { get; set; }
        [ForeignKey("CategoryId")] public Category Category { get; set; }
        [Required] public string Description { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Author { get; set; }
        [Required] public int NoPage { get; set; }

        [Range(10, 10000)]
        [DataType(DataType.Currency)]
        [Required]
        public double Price { get; set; }
        public string ImageUrl { get; set; }
        [Required] public string ISBN { get; set; }
    }
}