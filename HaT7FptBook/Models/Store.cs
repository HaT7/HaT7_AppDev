using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaT7FptBook.Models
{
    public class Store
    {
        [Key] public int Id { get; set; }
        [Required] public string StoreOwnerId { get; set; }
        [ForeignKey("StoreOwnerId")] public ApplicationUser ApplicationUser { get; set; }
        [Required] public string Name { get; set; }
    }
}