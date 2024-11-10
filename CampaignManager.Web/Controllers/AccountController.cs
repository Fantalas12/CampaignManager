using CampaignManager.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CampaignManager.Web.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager,
                                      IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /*
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Jwt:Key"));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        } */

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                //Since we are using email as the username, FindByNameAsync will search for the user by email.
                var user = await _userManager.FindByNameAsync(vm.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Login failed!");
                    return View(vm);
                }

                var result = await _signInManager.PasswordSignInAsync(user, vm.Password, isPersistent : true, lockoutOnFailure : false);

                if (result.Succeeded)
                {
                   //Generate JWT token
                   //var token = GenerateJwtToken(user);
                   //Return the token to the client as a JSON response
                   //return Ok(new { token });
                    //Don't use JWT, just redirect to the returnUrl
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError("", "Login failed!");
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                //Since we are using email as the username, we will set the email as the username. This ensures that the email is unique.
                var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
                var result = await _userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToLocal(returnUrl);
                }

                // Add errors to ModelState
                //foreach (var error in result.Errors) { ModelState.AddModelError("", error.Description); }
                //TODO - Handle different error types
                ModelState.AddModelError("", "Registration failed!");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }


    }
}
