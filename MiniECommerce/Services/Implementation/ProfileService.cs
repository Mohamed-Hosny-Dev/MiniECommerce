using Microsoft.AspNetCore.Identity;
using MiniECommerce.Models;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Implementation
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<UserProfileVM?> GetUserProfileAsync(string userId) {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
        }


        public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UserProfileVM model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found.");
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return (true, "Profile updated successfully.");
            else
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<bool> DeleteUserAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");


            if (isAdmin)
            {
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}