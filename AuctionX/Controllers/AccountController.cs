using AuctionX.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AuctionX.ViewModels;
using Microsoft.AspNetCore.Identity;


namespace AuctionX.Controllers
{
    public class AccountController : Controller
    {

        private readonly SportsAuctionContext _context;

        public AccountController(SportsAuctionContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login credentials.");
                return View(model);
            }

            // Use PasswordHasher to verify password
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Success)
            {
                // Setup claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("WelcomeMessage", $"Welcome back, {user.Name}!");

                return await RedirectToDashboard(user.Role, user.Id);
            }

            // Password incorrect
            TempData["InstructionWarning"] = "🤨 Yo, Stupid — follow the damn instructions! log in with Google if you are new player";
            ModelState.AddModelError("", "Invalid login credentials.");
            return View(model);
        }

        public IActionResult RegisterPlayer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPlayer(RegisterPlayerViewModel model, IFormFile Photo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle image upload
            string? fileName = null;
            if (Photo != null && Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                fileName = Path.GetFileName(Photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }
            }

            // Hash password
            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Role = "Player"
            };
            user.Password = hasher.HashPassword(user, model.Password); // 🔐 Hash the password

            // Save user
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Save player profile
            var player = new Player
            {
                Id = user.Id,
                UserId = user.Id,
                MobileNumber = model.MobileNumber,
                Description = model.Description,
                Photo = fileName,
                AvailabilityStatus = "Available"
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }



        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && !string.IsNullOrEmpty(user.Role))
            {
                var claimsList = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claimsList, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("WelcomeMessage", $"Welcome back, {user.Name}!");

                return await RedirectToDashboard(user.Role, user.Id); // FIXED HERE
            }

            TempData["Email"] = email;
            TempData["Name"] = name;

            return RedirectToAction("RegisterPlayer");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        private async Task<IActionResult> RedirectToDashboard(string role, int userId)
        {
            var playerId = await _context.Players
     .Where(p => p.UserId == userId)
     .Select(p => p.Id)
     .FirstOrDefaultAsync();

            var isBidder = await _context.Bidders.AnyAsync(b => b.PlayerId == playerId);


            if (role == "Player" && isBidder)
            {
                return RedirectToAction("Dashboard", "PlayerBidder");
            }

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Organizer" => RedirectToAction("Dashboard", "Organizer"),
                "Player" => RedirectToAction("Dashboard", "Player"),
                _ => RedirectToAction("Login")
            };
        }
    }
}
