using System.ComponentModel.DataAnnotations;

namespace RizkyAPI.Models
{
    public class POEntity
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int ItemId { get; set; }
        [Required]
        public int SupplierId { get; set; }
        [Required]
        public double Qty { get; set; }
        [Required]
        [StringLength(maximumLength: 50, ErrorMessage = "Max UnitType Char 50")]
        public string UnitType { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        [StringLength(maximumLength: 100, ErrorMessage = "Max CreatedBy Char 100")]
        public string CreatedBy { get; set; }
    }
}
