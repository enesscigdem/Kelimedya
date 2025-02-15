using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Paragraph.Core.IdentityEntities;
using Paragraph.Core.Models;
using System.Linq;

namespace Paragraph.WebAPI.Controllers
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