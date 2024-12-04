using API.Dtos;

namespace API.Services
{
    public interface IUserService
    {
        Task<PagedResponse<UserDetailDto>> GetUsersAsync(PaginationParams param);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    }
} 