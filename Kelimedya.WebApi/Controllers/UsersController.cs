using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Kelimedya.Core.IdentityEntities;
using Kelimedya.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Http;

namespace Kelimedya.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<CustomUser> _userManager;

        public UsersController(UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";
                userDtos.Add(new UserDto
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                    FullName = $"{user.Name} {user.Surname}",
                    ProfilePicture = user.ProfilePicture,
                    Role = role,
                    CreatedAt = user.CreatedAt,
                    TeacherId = user.TeacherId,
                    PhoneNumber = user.PhoneNumber,
                    ClassGrade = user.ClassGrade
                });
            }

            return Ok(userDtos);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? RoleNames.User;

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                FullName = $"{user.Name} {user.Surname}",
                ProfilePicture = user.ProfilePicture,
                Role = role,
                CreatedAt = user.CreatedAt,
                PhoneNumber = user.PhoneNumber,
                ClassGrade = user.ClassGrade,
                TeacherId = user.TeacherId
            };

            return Ok(userDto);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "Email is already in use." });
            }

            var newUser = new CustomUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                TeacherId = dto.TeacherId,
                IsActive = true,
                PhoneNumber = dto.PhoneNumber,
                ClassGrade = dto.ClassGrade,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { Message = "User creation failed.", Errors = createResult.Errors.Select(e => e.Description) });
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, dto.Role);
            if (!roleResult.Succeeded)
            {
                return BadRequest(new { Message = "User created but role assignment failed.", Errors = roleResult.Errors.Select(e => e.Description) });
            }

            return Ok(new { Message = "User created successfully." });
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UpdateUserDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { Message = "Invalid user ID." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.TeacherId = dto.TeacherId;
            user.PhoneNumber = dto.PhoneNumber;
            user.ClassGrade = dto.ClassGrade;

            if (dto.ImageFile != null)
            {
                user.ProfilePicture = await SaveFileAsync(dto.ImageFile, "profiles");
            }
            
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new { Message = "User update failed.", Errors = updateResult.Errors.Select(e => e.Description) });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault() ?? RoleNames.User;
            if (currentRole != dto.Role)
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRole);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(new { Message = "Failed to remove current role.", Errors = removeResult.Errors.Select(e => e.Description) });
                }
                var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addResult.Succeeded)
                {
                    return BadRequest(new { Message = "Failed to add new role.", Errors = addResult.Errors.Select(e => e.Description) });
                }
            }

            return Ok(new { Message = "User updated successfully." });
        }
        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "Kullanıcı bulunamadı." });
            }

            user.IsActive = false;
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Kullanıcı silinemedi.", Errors = result.Errors.Select(e => e.Description) });
            }
            return Ok(new { Message = "Kullanıcı başarıyla silindi." });
        }

        private async Task<string?> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"{Request.Scheme}://{Request.Host}/uploads/{folder}/{fileName}";
        }
    }
}
