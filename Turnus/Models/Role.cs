using System.ComponentModel.DataAnnotations;

namespace Turnus.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}