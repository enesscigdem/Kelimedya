using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Kelimedya.Core.IdentityEntities;
using Kelimedya.Core.Models;
using System.Linq;

namespace Kelimedya.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<CustomRole> _roleManager;
        public RolesController(RoleManager<CustomRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: api/roles
        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id.ToString(),
                Name = r.Name,
            }).ToList();
            return Ok(roleDtos);
        }
    }
}