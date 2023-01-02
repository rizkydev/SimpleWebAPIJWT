using System.ComponentModel.DataAnnotations;

namespace RizkyAPI.Models
{
    public class SupplierEntity
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 100, ErrorMessage = "Max SupplierName Char 100")]
        public string SupplierName { get; set; }
        [StringLength(maximumLength: 500, ErrorMessage = "Max SupplierAddress Char 500")]
        public string SupplierAddress { get; set; }
        [StringLength(maximumLength: 100, ErrorMessage = "Max SupplierContactPerson Char 100")]
        public string SupplierContactPerson { get; set; }
        [StringLength(maximumLength: 100, ErrorMessage = "Max SupplierContactPhone Char 100")]
        public string SupplierContactPhone { get; set; }
    }
}