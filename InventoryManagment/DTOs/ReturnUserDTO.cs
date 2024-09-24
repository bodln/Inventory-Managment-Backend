using System.ComponentModel.DataAnnotations;

namespace InventoryManagment.DTOs
{
    public class ReturnUserDTO
    {

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfHire { get; set; }
        public float Salary { get; set; }
    }
}
