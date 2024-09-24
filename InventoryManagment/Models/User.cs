using Microsoft.AspNetCore.Identity;

namespace InventoryManagment.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfHire { get; set; }
        public float Salary { get; set; }
    }
}
