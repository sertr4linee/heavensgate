using API.Models;

namespace API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<AppUser>> GetUsersAsync(int skip, int take);
        Task<int> GetTotalUsersAsync();
    }
} 