using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto{
                    IsSuccess = false,
                    Message = "Invalid credentials"
                });
            }


            var result = await _userManager.CheckPasswordAsync(user,loginDto.Password);
            if (!result)
            {
                return Unauthorized(new AuthResponseDto{
                    IsSuccess = false,
                    Message = "Invalid credentials"
                });
            }

            var token = GenerateToken(user);

            return Ok(new AuthResponseDto{
                Token = token,
                IsSuccess = true,
                Message = "User logged in successfully"
            });
        }

        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JWTSetting").GetSection("secretKey").Value!);
            var roles = _userManager.GetRolesAsync(user).Result;
            List<Claim> claims =
            [
                new (JwtRegisteredClaimNames.Email,user.Email??""),
                new (JwtRegisteredClaimNames.Name,user.FullName??""),
                new (JwtRegisteredClaimNames.NameId,user.Id??""),
                new (JwtRegisteredClaimNames.Aud,_configuration.GetSection("JWTSetting").GetSection("validAudience").Value!),
                new (JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWTSetting").GetSection("validIssuer").Value!),
            ];

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role,role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetails()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);
            if (user is null)
            {
                return NotFound(new AuthResponseDto{
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            return Ok(new UserDetailDto{
                Id = user.Id,
                FullName = user.FullName!,
                Email = user.Email!,
                Roles = [..await _userManager.GetRolesAsync(user)],
                PhoneNumber = user.PhoneNumber,
                TwoFactorEnabled = user.TwoFactorEnabled,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<PagedResponse<UserDetailDto>>> GetUsers([FromQuery] PaginationParams param)
        {
            var query = _userManager.Users;
            var total = await query.CountAsync();
            var users = await query
                .Skip((param.PageNumber - 1) * param.PageSize)
                .Take(param.PageSize)
                .Select(user => new UserDetailDto{
                    Id = user.Id,
                    FullName = user.FullName!,
                    Email = user.Email!,
                    Roles = _userManager.GetRolesAsync(user).Result.ToArray()
                }).ToListAsync();
            
            return Ok(new PagedResponse<UserDetailDto>(users, param.PageNumber, param.PageSize, total));
        }

        // [HttpPost("refresh-token")]
        // public async Task<ActionResult<AuthResponseDto>> RefreshToken()
        // {
        //     var refreshToken = Request.Cookies["refreshToken"];
        //     // Logique de validation et renouvellement du token
        // }
    }
}