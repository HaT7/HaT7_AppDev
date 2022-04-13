using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }
        
        [Required] public int StoreId { get; set; }
        
        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; }

        public DateTime CreateAt { get; set; }

        public Category()
        {
            CreateAt = DateTime.Now;
        }
    }
}