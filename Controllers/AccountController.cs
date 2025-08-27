using Microsoft.AspNetCore.Mvc;
using HealthCareApp.Services;
using HealthCareApp.Models;
using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserID") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.AuthenticateUserAsync(model.Username, model.Password);
                    
                    if (user != null)
                    {
                        // Set session variables
                        HttpContext.Session.SetString("UserID", user.UserID.ToString());
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("UserType", user.UserType);

                        _logger.LogInformation($"User {user.Username} logged in successfully as {user.UserType}");

                        // Redirect based on user type
                        return user.UserType switch
                        {
                            "Admin" => RedirectToAction("Index", "Admin"),
                            "Doctor" => RedirectToAction("Index", "Doctor"),
                            "Patient" => RedirectToAction("Index", "Patient"),
                            _ => RedirectToAction("Index", "Home")
                        };
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login");
                    ModelState.AddModelError("", "An error occurred during login. Please try again.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User
                    {
                        Username = model.Username,
                        Password = model.Password
                    };

                    var result = await _userService.CreateUserAsync(user, "Patient");
                    
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Registration successful! Please login.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Registration failed. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration");
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");
            
            // Clear session
            HttpContext.Session.Clear();
            
            _logger.LogInformation($"User {username} logged out");
            
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }

    // View Models
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
