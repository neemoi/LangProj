using Application.Services.Interfaces.IRepository.Profile;
using Domain.Models;
using LangLearningAPI.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Persistance.Repository.Userfsf
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserManager<Users> userManager, ILogger<UserRepository> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Users> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Attempted to get user with empty email");
                    throw new ArgumentException("Email cannot be empty", nameof(email));
                }

                _logger.LogInformation("Fetching user by email: {UserEmail}", email);

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("User with email {UserEmail} not found", email);
                    throw new NotFoundException($"User with email {email} not found", "USER_NOT_FOUND");
                }

                return user;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error fetching user by email: {UserEmail}", email);
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to get user by email",
                    "GET_USER_BY_EMAIL_FAILED",
                    ex);
            }
        }

        public async Task<Users> GetUserByUserNameAsync(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    _logger.LogWarning("Attempted to get user with empty username");
                    throw new ArgumentException("Username cannot be empty", nameof(userName));
                }

                _logger.LogInformation("Fetching user by username: {UserName}", userName);

                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                {
                    _logger.LogWarning("User with username {UserName} not found", userName);
                    throw new NotFoundException($"User with username {userName} not found", "USER_NOT_FOUND");
                }

                return user;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error fetching user by username: {UserName}", userName);
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to get user by username",
                    "GET_USER_BY_USERNAME_FAILED",
                    ex);
            }
        }

        public async Task<Users> GetUserByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("Attempted to get user with empty ID");
                    throw new ArgumentException("User ID cannot be empty", nameof(id));
                }

                _logger.LogInformation("Fetching user by ID: {UserId}", id);

                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    throw new NotFoundException($"User with ID {id} not found", "USER_NOT_FOUND");
                }

                return user;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error fetching user by ID: {UserId}", id);
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to get user",
                    "GET_USER_FAILED",
                    ex);
            }
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all users");

                var users = await _userManager.Users.ToListAsync();

                if (users == null || !users.Any())
                {
                    _logger.LogWarning("No users found in database");
                    return Enumerable.Empty<Users>();
                }

                return users;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error fetching all users");
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to get users",
                    "GET_USERS_FAILED",
                    ex);
            }
        }

        public async Task<IdentityResult> UpdateUserAsync(Users user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("Attempted to update null user");
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                }

                _logger.LogInformation("Updating user with ID: {UserId}", user.Id);

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to update user {UserId}. Errors: {Errors}", user.Id, errors);
                    throw new IdentityException("User update failed", "USER_UPDATE_FAILED", result.Errors.ToString());
                }

                _logger.LogInformation("User {UserId} updated successfully", user.Id);

                return result;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", user?.Id);
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to update user",
                    "UPDATE_USER_FAILED",
                    ex);
            }
        }

        public async Task<IEnumerable<Users>> GetBlockedUsersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all blocked users");

                var blockedUsers = await _userManager.Users
                    .Where(u => u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
                    .ToListAsync();

                if (!blockedUsers.Any())
                {
                    _logger.LogInformation("No blocked users found");
                    return Enumerable.Empty<Users>();
                }

                return blockedUsers;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error fetching blocked users");
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to get blocked users",
                    "GET_BLOCKED_USERS_FAILED",
                    ex);
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("Attempted to delete user with empty ID");
                    throw new ArgumentException("User ID cannot be empty", nameof(id));
                }

                _logger.LogInformation("Deleting user with ID: {UserId}", id);

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                    throw new NotFoundException($"User with ID {id} not found", "USER_NOT_FOUND");
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to delete user {UserId}. Errors: {Errors}", id, errors);
                    throw new IdentityException("User deletion failed", "USER_DELETION_FAILED", result.Errors.ToString());
                }

                _logger.LogInformation("User {UserId} deleted successfully", id);

                return result;
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                throw new ApiException(
                    (int)HttpStatusCode.InternalServerError,
                    "Failed to delete user",
                    "DELETE_USER_FAILED",
                    ex);
            }
        }
    }
}