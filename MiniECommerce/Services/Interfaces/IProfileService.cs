using MiniECommerce.ViewModels;
namespace MiniECommerce.Services.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileVM?> GetUserProfileAsync(string userId);
        Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UserProfileVM model);
        Task<bool> DeleteUserAccountAsync(string userId);
    }
}
