using System.ComponentModel.DataAnnotations;

namespace RizkyAPI.Models
{
    public class ItemEntity
    {
        [Required]
        public long Id { get; set; }
        [Required]
        [StringLength(maximumLength : 100, ErrorMessage = "Max Item Code Char 100")]
        public string ItemCode { get; set; }
        [Required]
        [StringLength(maximumLength: 200, ErrorMessage = "Max Item Name Char 200")]
        public string ItemName { get; set; }
        [StringLength(maximumLength: 200, ErrorMessage = "Max Item Description Char 200")]
        public string? ItemDesc { get; set; }
    }
}
