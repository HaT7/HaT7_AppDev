using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class OrderHeader
    {
        [Key] public int Id { get; set; }
        
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")] public ApplicationUser ApplicationUser { get; set; }

        [Required] public double Total { get; set; }
        [Required] public string Address { get; set; }
        [Required] public DateTime OderDate { get; set; }

        public DateTime CreateAt { get; set; }

        public OrderHeader()
        {
            CreateAt = DateTime.Now;
        }
    }
}