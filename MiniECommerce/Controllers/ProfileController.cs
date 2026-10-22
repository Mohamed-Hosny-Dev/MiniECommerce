using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniECommerce.Models;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;
namespace MiniECommerce.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(IProfileService profileService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _profileService = profileService;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userID = _userManager.GetUserId(User);
            var (Success, Message) = await _profileService.UpdateProfileAsync(userID, model);
            if (Success)
            {
                TempData["SuccessMessage"] = Message;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", Message);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            if (User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Admins cannot delete their own accounts. Please transfer admin rights first.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var isDeleted = await _profileService.DeleteUserAccountAsync(userId);

            if (isDeleted)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Could not delete account. If you have active orders or you are an Admin, you cannot perform this action.";
            return RedirectToAction(nameof(Index));
        }
    }
}
