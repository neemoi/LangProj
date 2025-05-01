using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Interfaces.IRepository.Profile
{
    public interface IUserRepository
    {
        Task<Users> GetUserByIdAsync(string id);

        Task<Users> GetUserByEmailAsync(string email);

        Task<Users> GetUserByUserNameAsync(string userName);

        Task<IEnumerable<Users>> GetAllUsersAsync();

        Task<IdentityResult> UpdateUserAsync(Users user);

        Task<IEnumerable<Users>> GetBlockedUsersAsync();

        Task<IdentityResult> DeleteUserAsync(string id);
    }
}
