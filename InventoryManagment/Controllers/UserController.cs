using InventoryManagment.DTOs;
using InventoryManagment.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserAccount _userAccount;
        private readonly IConfiguration _configuration;

        public UserController(IUserAccount userAccount,
            IConfiguration configuration)
        {
            _userAccount = userAccount;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDTO userDTO)
        {
            var response = await _userAccount.CreateAccount(userDTO);
            return Ok(response);
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var response = await _userAccount.LoginAccount(loginDTO);
            return Ok(response);
        }

        [HttpGet("GetAll"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _userAccount.GetAll();
            return Ok(response);
        }


        [HttpDelete("RemoveAllUsers"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAllUsers()
        {
            var response = await _userAccount.RemoveAllUsers();
            return Ok(response);
        }

        [HttpDelete("RemoveAllRoles"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAllRoles()
        {
            var response = await _userAccount.RemoveAllRoles();
            return Ok(response);
        }

        [HttpPost("AddRole"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(string email, string roleName)
        {
            var response = await _userAccount.AddRole(email, roleName);
            return Ok(response);
        }

        [HttpPost("RemoveRole"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(string email, string roleName)
        {
            var response = await _userAccount.RemoveRole(email, roleName);
            return Ok(response);
        }
    }
}
