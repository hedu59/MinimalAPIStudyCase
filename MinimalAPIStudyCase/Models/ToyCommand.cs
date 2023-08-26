using MinimalAPIStudyCase.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPIStudyCase.Models
{
    public class ToyCommand
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(300)]
        public string? Description { get; set; } = null;

        [Required]
        [Range(1,9999,ErrorMessage = "The price must be between 1 and 9999")]
        public decimal? Price { get; set; }

        [Required]
        [EnumDataType(typeof(ETypeToy),ErrorMessage ="The type must have values from TypeToy (Ex:1,2,3,4)")]
        public ETypeToy TypeToy { get; set; }
    }
}
