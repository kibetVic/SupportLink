using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SupportLink.Models
{
    [Index(nameof(OrganizationId), nameof(Name), IsUnique = true)]
    public class Category
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(120)] public string Name { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
    }
}
