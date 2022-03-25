using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class OderDetail
    {
        [Key] public int Id { get; set; }

        [Required] public int OrderHeaderId { get; set; }
        [ForeignKey("OrderHeaderId")] public OrderHeader OrderHeader { get; set; }

        [Required] public int ProductId { get; set; }
        [ForeignKey("ProductId")] public Product Product { get; set; }

        [Required] public double Price { get; set; }
        [Required] public int Quantity { get; set; }

        public DateTime CreateAt { get; set; }

        public OderDetail()
        {
            CreateAt = DateTime.Now;
        }
    }
}