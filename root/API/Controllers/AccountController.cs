using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;

        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser{
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email,
            };

            var result = await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (registerDto.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "User");
            } else {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user,role);
                }
            }

            return Ok(new AuthResponseDto{
                IsSuccess = true,
                Message = "User registered successfully"
            });
            
        }

    }
}