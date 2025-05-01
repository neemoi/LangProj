using Application.DtoModels.AdminUsers;
using Application.DtoModels.User;

namespace Application.Services.Interfaces.IServices.Profile
{
    public interface IUserService
    {
        Task<UserByIdDto> GetUserByIdAsync(string id);

        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task<UserByIdDto> GetUserByUserNameAsync(string userName);

        Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);

        Task<UserResponseDto> DeleteUserAsync(string id, string currentUserId);

        Task<IEnumerable<UserDto>> GetBlockedUsersAsync();

        Task<UserByIdDto> GetUserByEmailAsync(string email);
    }
}
