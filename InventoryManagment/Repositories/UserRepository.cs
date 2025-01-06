using InventoryManagment.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagment.Models;
using InventoryManagment.DTOs;

namespace InventoryManagment.Repositories
{
    public interface IUserAccount
    {
        Task<MessageReturnDTO> CreateAccount(UserDTO userDTO);
        Task<AuthDTO> LoginAccount(LoginDTO loginDTO);
        Task<List<ReturnUserDTO>> GetAll();
        Task<MessageReturnDTO> Edit(ReturnUserDTO userDTO);
        Task<MessageReturnDTO> RemoveAllUsers();
        Task<MessageReturnDTO> RemoveUser(string email);
        Task<MessageReturnDTO> RemoveAllRoles();
        Task<MessageReturnDTO> AddRole(string email, string roleName);
        Task<MessageReturnDTO> RemoveRole(string email, string roleName);
    }

    public class UserRepository : IUserAccount
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public UserRepository(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
        }

        public async Task<MessageReturnDTO> CreateAccount(UserDTO userDTO)
        {
            var newUser = new User()
            {
                Email = userDTO.Email,
                PasswordHash = userDTO.Password,
                UserName = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Address = userDTO.Address,
                DateOfBirth = userDTO.DateOfBirth,
                DateOfHire = userDTO.DateOfHire,
                Salary = userDTO.Salary
            };

            MessageReturnDTO message = new MessageReturnDTO();

            var user = await _userManager.FindByEmailAsync(newUser.Email);
            if (user is not null)
            {
                message.Message = "Email registered already";
                return message;
            }

            var createUser = await _userManager.CreateAsync(newUser!, userDTO.Password);
            if (!createUser.Succeeded)
            {
                message.Message = "Error occurred.. please try again";
                return message;
            }

            var checkAdmin = await _roleManager.FindByNameAsync("Admin");
            if (checkAdmin is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Warehouseman" });
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Manager" });

                await _userManager.AddToRoleAsync(newUser, "Warehouseman");
                await _userManager.AddToRoleAsync(newUser, "Manager");
                await _userManager.AddToRoleAsync(newUser, "Admin");
                message.Message = "Account Created";

                return message;
            }
            else
            {
                await _userManager.AddToRoleAsync(newUser, "Warehouseman");
                message.Message = "Account Created";

                return message;
            }
        }

        public async Task<AuthDTO> LoginAccount(LoginDTO loginDTO)
        {
            AuthDTO authDTO = new AuthDTO();

            if (loginDTO == null)
                authDTO.token = "Login container is empty"; 

            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
                authDTO.token = "User not found";

            bool checkUserPasswords = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPasswords)
            {
                authDTO.token = "Invalid email/password";
                authDTO.code = 401;
            }
            else
            {
                var getUserRoles = await _userManager.GetRolesAsync(getUser);
                var userSession = new UserSession(getUser.Email, getUserRoles.ToList());
                string token = GenerateToken(userSession);
                authDTO.token = token;
                authDTO.code = 200;
            }

            return authDTO;
        }

        public async Task<List<ReturnUserDTO>> GetAll()
        {
            var users = _context.Users.ToList();
            var returnUsers = new List<ReturnUserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var returnUser = new ReturnUserDTO
                {
                    Email = user.Email,
                    Roles = roles.ToList(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    DateOfHire = user.DateOfHire,
                    Salary = user.Salary
                };
                returnUsers.Add(returnUser);
            }

            return returnUsers;
        }

        public async Task<MessageReturnDTO> RemoveUser(string email)
        {
            MessageReturnDTO message = new MessageReturnDTO();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) message.Message = "User not found.";

            var result = await _userManager.DeleteAsync(user);
            message.Message = result.Succeeded ? $"{email} user deleted successfully." : $"Failed to delete {email} user.";

            return message;
        }

        public async Task<MessageReturnDTO> RemoveAllUsers()
        {
            MessageReturnDTO message = new MessageReturnDTO();
            var allUsers = _context.Users.ToList();
            foreach (var user in allUsers)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    message.Message = "Failed to delete all users.";
                    return message;
                }
            }

            message.Message = "Successfully deleted all users.";
            return message;
        }

        public async Task<MessageReturnDTO> RemoveAllRoles()
        {
            MessageReturnDTO message = new MessageReturnDTO();
            var allRoles = _roleManager.Roles.ToList();
            foreach (var role in allRoles)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    message.Message = "Failed to delete all roles.";
                    return message;
                }
            }

            message.Message = "Successfully deleted all roles.";
            return message;
        }

        public async Task<MessageReturnDTO> AddRole(string email, string roleName)
        {
            MessageReturnDTO message = new MessageReturnDTO();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) message.Message = "User not found.";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                message.Message = "Role not found.";
                return message;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            message.Message = result.Succeeded ? $"{roleName} role added successfully." : $"Failed to add {roleName} role.";

            return message;
        }

        public async Task<MessageReturnDTO> RemoveRole(string email, string roleName)
        {
            MessageReturnDTO message = new MessageReturnDTO();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) message.Message = "User not found.";

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                message.Message = $"User is not in the {roleName} role.";
                return message;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            message.Message = result.Succeeded ? $"{roleName} role removed successfully." : $"Failed to remove {roleName} role.";

            return message;
        }

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };
            userClaims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var audiences = _config.GetSection("Jwt:Audiences").Get<List<string>>();
            userClaims.AddRange(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<MessageReturnDTO> Edit(ReturnUserDTO userDTO)
        {
            MessageReturnDTO message = new MessageReturnDTO();
            List<string> roles = new List<string>
            {
                "Admin",
                "Manager",
                "Warehouseman"
            };

            message.Message = "Edit successful.";

            try
            {
                User user = await _userManager.FindByEmailAsync(userDTO.Email);

                user.Email = userDTO.Email;
                user.FirstName = userDTO.FirstName;
                user.LastName = userDTO.LastName;
                user.Salary = userDTO.Salary;
                user.Address = userDTO.Address;

                foreach (string role in roles)
                {
                    if (userDTO.Roles.Contains(role))
                    {
                        if (!await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                    else
                    {
                        if (await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.RemoveFromRoleAsync(user, role);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                message.Message = "Failed to edit user info. Message: " + ex.Message;
            }


            return message;
        }
    }
    public record UserSession(string Email, List<string> Roles);
}
