using AuctionX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuctionX.Models;
using AuctionX.Services;
using AuctionX.ViewModels;
using System.Security.Claims;

namespace AuctionX.Controllers
{
    public class AdminController : Controller
    {
        private readonly SportsAuctionContext _context;
        private readonly IEmailService _emailService;
        public AdminController(SportsAuctionContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;   
        }
        // Dashboard Action
        public async Task<IActionResult> Dashboard()

        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.WelcomeMessage = HttpContext.Session.GetString("WelcomeMessage");
            HttpContext.Session.Remove("WelcomeMessage"); // Clear it after reading

            // Get total number of organizers, players, teams, and tournaments
            var totalOrganizers = await _context.Organizers.CountAsync();
            var totalPlayers = await _context.Players.CountAsync();
            var totalTeams = await _context.Teams.CountAsync();
            var totalTournaments = await _context.Tournaments.CountAsync();

            // Create a view model for the dashboard data
            var dashboardData = new AdminDashboardViewModel
            {
                TotalOrganizers = totalOrganizers,
                TotalPlayers = totalPlayers,
                TotalTeams = totalTeams,
                TotalTournaments = totalTournaments
            };

            return View(dashboardData);
        }

        public IActionResult Profile()
        {
            // Fetch the user ID from the current authenticated user
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if userId is valid
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if the user is not authenticated
            }

            // Fetch the admin details from the database using the userId
            var admin = _context.Admins.Include(a => a.User) // Assuming Admin has a related User entity
                                        .FirstOrDefault(a => a.User.Id == userId);

            // If no admin found, return a NotFound result
            if (admin == null)
            {
                return NotFound(); // Or you could return an error view
            }

            // Pass the admin details to the view
            return View(admin);
        }


        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Use Claims-based identity to get the user ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = _context.Admins
                                .Include(a => a.User)
                                .FirstOrDefault(a => a.UserId == userId);

            if (admin == null)
            {
                return NotFound();
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(admin.User, admin.User.Password, oldPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Old password is incorrect.");
                return View("Profile", admin);
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation do not match.");
                return View("Profile", admin);
            }

            admin.User.Password = hasher.HashPassword(admin.User, newPassword);

            _context.SaveChanges();
            TempData["InstructionWarning"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }
        // GET: Organizers List
        public IActionResult Organizers(string searchQuery)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            var organizers = _context.Organizers.Include(o => o.User).AsQueryable();

            // If there is a search query, filter the organizers by name or email
            if (!string.IsNullOrEmpty(searchQuery))
            {
                organizers = organizers.Where(o => o.User != null &&
                                                   (o.User.Name.Contains(searchQuery) || o.User.Email.Contains(searchQuery)));
            }

            // Fetch the result and pass it to the view
            ViewBag.Success = TempData["Success"];
            return View(organizers.ToList());
        }


        // GET: Organizer/Create
        public IActionResult CreateOrganizer()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganizer(CreateOrganizerViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                bool emailExists = _context.Users.Any(u => u.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Hash the password
                var hasher = new PasswordHasher<User>();
                var tempUser = new User(); // Needed for hashing
                string hashedPassword = hasher.HashPassword(tempUser, model.Password);

                // Create new user
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = hashedPassword,
                    Role = "Organizer"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create organizer
                var organizer = new Organizer
                {
                    UserId = user.Id,
                    MobileNumber = model.MobileNumber
                };

                _context.Organizers.Add(organizer);
                await _context.SaveChangesAsync();

                // Prepare email content (DO NOT send plain password if security is critical)
                var subject = "Welcome to AuctionX";
                var body = $@"
            <h2>Welcome, {user.Name}!</h2>
            <p>Your account has been created successfully.</p>
            <p>Your login credentials are:</p>
            <ul>
                <li>Username: {user.Email}</li>
                <li>Password: {model.Password}</li>
            </ul>
            <p>You can log in with these credentials. Please change your password after logging in for the first time.</p>
            <p>Best Regards,<br/>AuctionX Team</p>
        ";

                await _emailService.SendEmailAsync(user.Email, subject, body);

                TempData["InstructionWarning"] = "✅ Organizer account created and credentials sent via email!";
                return RedirectToAction("Organizers", "Admin");

            }

            return View(model);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var organizer = await _context.Organizers.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);

            if (organizer == null)
            {
                return NotFound();
            }

            // Create a view model for editing
            var model = new OrganizerEditViewModel
            {
                Id = organizer.Id,
                Name = organizer.User.Name,
                Email = organizer.User.Email,
                MobileNumber = organizer.MobileNumber
            };

            return View(model);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,MobileNumber")] OrganizerEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var organizer = await _context.Organizers.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);

                if (organizer == null)
                {
                    return NotFound();
                }

                // Update the organizer details
                organizer.User.Name = model.Name;
                organizer.User.Email = model.Email;
                organizer.MobileNumber = model.MobileNumber;

                _context.Update(organizer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Organizer updated successfully!";
                TempData["InstructionWarning"] = "✅ Organizer updated successfully!";

                return RedirectToAction(nameof(Organizers));
            }

            return View(model);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (id == null)
            {
                return NotFound();
            }

            var organizer = await _context.Organizers.Include(o => o.User)
                                                     .FirstOrDefaultAsync(o => o.Id == id);

            if (organizer == null)
            {
                return NotFound();
            }

            return View(organizer);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organizer = await _context.Organizers.Include(o => o.User)
                                                     .FirstOrDefaultAsync(o => o.Id == id);
            if (organizer != null)
            {
                _context.Organizers.Remove(organizer);
                _context.Users.Remove(organizer.User); // Remove the related user as well
                await _context.SaveChangesAsync();

                TempData["Success"] = "Organizer deleted successfully!";
                TempData["InstructionWarning"] = "✅ Organizer deleted successfully!";
            }

            // Redirect to the Organizers page after deletion
            return RedirectToAction(nameof(Organizers));
        }

        // GET: Players List
        public IActionResult Players(string searchQuery)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            var players = _context.Players.Include(p => p.User).AsQueryable();

            // If there is a search query, filter the players by name or email
            if (!string.IsNullOrEmpty(searchQuery))
            {
                players = players.Where(p => p.User != null &&
                                              (p.User.Name.Contains(searchQuery) || p.User.Email.Contains(searchQuery)));
            }

            ViewBag.Success = TempData["Success"];
            return View(players.ToList());
        }


        // GET: Admin/Edit/5
        public async Task<IActionResult> EditPlayer(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
            {
                return NotFound();
            }

            // Create a view model for editing
            var model = new PlayerEditViewModel
            {
                Id = player.Id,
                Name = player.User.Name,
                Email = player.User.Email,
                MobileNumber = player.MobileNumber,
                Description = player.Description,
                AvailabilityStatus = player.AvailabilityStatus
            };

            return View(model);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPlayer(int id, [Bind("Id,Name,Email,MobileNumber,Description,AvailabilityStatus")] PlayerEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var player = await _context.Players.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

                if (player == null)
                {
                    return NotFound();
                }

                // Update the player details
                player.User.Name = model.Name;
                player.User.Email = model.Email;
                player.MobileNumber = model.MobileNumber;
                player.Description = model.Description;
                player.AvailabilityStatus = model.AvailabilityStatus;

                _context.Update(player);
                await _context.SaveChangesAsync();

                
                TempData["InstructionWarning"] = "✅ Player updated successfully!";
                return RedirectToAction(nameof(Players));
            }

            return View(model);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> DeletePlayer(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.Include(p => p.User)
                                               .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("DeletePlayer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlayerConfirmed(int id)
        {
            var player = await _context.Players.Include(p => p.User)
                                               .FirstOrDefaultAsync(p => p.Id == id);
            if (player != null)
            {
                _context.Players.Remove(player);
                _context.Users.Remove(player.User); // Remove the related user as well
                await _context.SaveChangesAsync();

              
                TempData["InstructionWarning"] = "✅ Player deleted successfully!";
            }

            return RedirectToAction(nameof(Players));
        }

        // GET: Admin/Tournaments
        public IActionResult Tournaments(string searchQuery)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            var tournaments = _context.Tournaments
                .Include(t => t.Organizer).ThenInclude(o => o.User)
                .Include(t => t.Sport)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                tournaments = tournaments.Where(t =>
                    t.Name.Contains(searchQuery) ||
                    t.Venue.Contains(searchQuery) ||
                    t.Organizer.User.Name.Contains(searchQuery) ||
                    t.Sport.Name.Contains(searchQuery));
            }

            return View(tournaments.ToList());
        }



        // GET: Admin/ViewTournament/5
        public async Task<IActionResult> ViewTournament(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer).ThenInclude(o => o.User)
                .Include(t => t.Sport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return NotFound();

            var bidMaster = await _context.BidMasters
                .FirstOrDefaultAsync(b => b.TournamentId == tournament.Id);

            var tournamentPlayers = await _context.TournamentPlayers
                .Where(tp => tp.TournamentId == tournament.Id)
                .ToListAsync();

            var teams = await _context.Teams
                .Where(t => t.TournamentId == tournament.Id)
                .ToListAsync();

            var captainNames = new Dictionary<int, string>();
            var captainRoles = new Dictionary<int, List<string>>();

            foreach (var team in teams)
            {
                if (team.CaptainId.HasValue)
                {
                    var captain = await _context.Bidders
                        .Include(b => b.Player)
                        .ThenInclude(p => p.User)
                        .FirstOrDefaultAsync(b => b.Id == team.CaptainId);

                    if (captain != null)
                    {
                        captainNames[team.Id] = captain.Player?.User?.Name;

                        var roles = await _context.BidderRoles
                            .Where(r => r.BidderId == captain.Id)
                            .Select(r => r.RoleName)
                            .ToListAsync();

                        captainRoles[team.Id] = roles;
                    }
                }
            }

            var viewModel = new TournamentDetailViewModel
            {
                Tournament = tournament,
                TournamentPlayers = tournamentPlayers,
                Teams = teams,
                CaptainNames = captainNames,
                CaptainRoles = captainRoles,
                BidMaster = bidMaster
            };

            return View(viewModel);
        }

        // GET: Admin/EditTournament/5
        public async Task<IActionResult> EditTournament(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return NotFound();

            ViewBag.Organizers = new SelectList(_context.Organizers.Include(o => o.User), "Id", "User.Name", tournament.OrganizerId);
            ViewBag.Sports = new SelectList(_context.Sports, "Id", "Name", tournament.SportId);

            return View(tournament);
        }

        // POST: Admin/EditTournament/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTournament(int id, Tournament model, IFormFile? logoFile)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTournament = await _context.Tournaments.FindAsync(id);
                    if (existingTournament == null) return NotFound();

                    // Update basic properties
                    existingTournament.Name = model.Name;
                    existingTournament.Date = model.Date;
                    existingTournament.Venue = model.Venue;
                    existingTournament.SportId = model.SportId;
                    existingTournament.OrganizerId = model.OrganizerId;

                    // Update logo only if a new file is provided
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                        var filePath = Path.Combine(folderPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(stream);
                        }

                        existingTournament.Logo = uniqueFileName;
                    }

                    _context.Update(existingTournament);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Tournament updated successfully!";
                    TempData["InstructionWarning"] = "✅ Tournament updated successfully!";

                    return RedirectToAction(nameof(Tournaments));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tournaments.Any(t => t.Id == model.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Organizers = new SelectList(_context.Organizers.Include(o => o.User), "Id", "User.Name", model.OrganizerId);
            ViewBag.Sports = new SelectList(_context.Sports, "Id", "Name", model.SportId);
            return View(model);
        }


        // GET: Admin/DeleteTournament/5
        public async Task<IActionResult> DeleteTournament(int? id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer).ThenInclude(o => o.User)
                .Include(t => t.Sport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return NotFound();

            return View(tournament);
        }

        // POST: Admin/DeleteTournament/5
        [HttpPost, ActionName("DeleteTournament")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTournamentConfirmed(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Tournament deleted successfully!";
                TempData["InstructionWarning"] = "✅ Tournament deleted successfully!";
            }

            return RedirectToAction(nameof(Tournaments));
        }

        public async Task<IActionResult> ViewPlayers(int tournamentId, string statusFilter, string searchQuery)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            var players = await _context.TournamentPlayers
                .Where(tp => tp.TournamentId == tournamentId)
                .Include(tp => tp.Player)
                .ThenInclude(p => p.User)
                .ToListAsync();

            var soldPlayerIds = await _context.TournamentPlayers
    .Where(tp => tp.TournamentId == tournamentId && tp.AvailabilityStatus == "SOLD")
    .Select(tp => tp.PlayerId)
    .ToListAsync();


            var viewModel = players.Select(tp => new PlayerStatusViewModel
            {
                PlayerName = tp.Player.User.Name,
                Status = soldPlayerIds.Contains(tp.PlayerId) ? "SOLD" : "UNSOLD"
            });

            // Filter by status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                viewModel = viewModel.Where(v => v.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by name search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                viewModel = viewModel.Where(v => v.PlayerName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.TournamentId = tournamentId;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchQuery = searchQuery;

            return View(viewModel.ToList());
        }


        [HttpPost]
        public async Task<IActionResult> BulkUploadPlayers(IFormFile file)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }


            if (file == null || file.Length == 0)
                return RedirectToAction("ManagePlayers");

            using var reader = new StreamReader(file.OpenReadStream());
            var hasher = new PasswordHasher<User>(); // Create once outside the loop

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split(',');

                if (values.Length < 5)
                    continue; // Invalid format, skip

                var email = values[1].Trim();

                // Skip duplicate emails
                if (_context.Users.Any(u => u.Email == email))
                    continue;

                var user = new User
                {
                    Name = values[0].Trim(),
                    Email = email,
                    Role = "Player"
                };

                // Hash the password before saving
                user.Password = hasher.HashPassword(user, values[2].Trim());

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Save to get User.Id

                var player = new Player
                {
                    Id = user.Id,
                    UserId = user.Id,
                    Photo = values.Length > 5 ? values[5].Trim() : null, // 6th column
                    MobileNumber = values[3].Trim(), // 4th column
                    Description = values.Length > 6 ? values[6].Trim() : null, // 7th column
                    AvailabilityStatus = values[4].Trim() // 5th column
                };

                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Players", "Admin");
        }


    }
}
