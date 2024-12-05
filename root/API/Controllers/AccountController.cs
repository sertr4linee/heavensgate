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
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using API.Data;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AccountController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AccountController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;

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
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(new AuthResponseDto{
                    IsSuccess = false,
                    Message = "Invalid credentials"
                });
            }

            var token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);
            
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.ExpiryDate
            });

            return Ok(new AuthResponseDto{
                Token = token,
                IsSuccess = true,
                Message = "User logged in successfully"
            });
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized("No refresh token provided");

                var allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<string[]>();
                if (!Request.Headers.TryGetValue("Origin", out var origin) || 
                    allowedOrigins == null || 
                    !allowedOrigins.Any(o => o == origin.ToString()))
                {
                    return Unauthorized("Invalid origin");
                }

                var storedToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsActive);

                if (storedToken == null || storedToken.IsExpired)
                {
                    if (storedToken != null)
                    {
                        _context.RefreshTokens.Remove(storedToken);
                        await _context.SaveChangesAsync();
                    }
                    Response.Cookies.Delete("refreshToken");
                    return Unauthorized("Invalid or expired refresh token");
                }

                var user = storedToken.User;
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    return Unauthorized("User not found or email not confirmed");
                }

                var newToken = GenerateToken(user);
                var newRefreshToken = GenerateRefreshToken(user.Id);

                storedToken.IsActive = false;
                _context.RefreshTokens.Add(newRefreshToken);
                await _context.SaveChangesAsync();

                Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newRefreshToken.ExpiryDate,
                    Path = "/api/account/refresh-token"
                });

                return Ok(new AuthResponseDto
                {
                    Token = newToken,
                    IsSuccess = true,
                    Message = "Token refreshed successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "An error occurred while refreshing the token"
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
                
                if (storedToken != null)
                {
                    storedToken.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                Response.Cookies.Delete("refreshToken");
            }

            return Ok(new { message = "Logged out successfully" });
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

        private RefreshToken GenerateRefreshToken(string userId)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = userId
            };
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
        [EnableRateLimiting("api")]
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
    }
}