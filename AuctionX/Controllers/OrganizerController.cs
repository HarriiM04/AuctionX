using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuctionX.Models;
using AuctionX.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace AuctionX.Controllers
{
    public class OrganizerController : Controller
    {
        private readonly SportsAuctionContext _context;

        public OrganizerController(SportsAuctionContext context)
        {
            _context = context;
        }

        //add method for profile
        // GET: Organizer/Profile
        public async Task<IActionResult> Profile()
        {
            // Assuming you have logged-in User ID via session or claims — here's a sample using Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);


            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
            {
                return NotFound();
            }

            return View(organizer);
        }


        // GET: Organizer/EditProfile
        public async Task<IActionResult> EditProfile(int id)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organizer == null)
            {
                return NotFound();
            }

            return View(organizer);
        }


        // POST: Organizer/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int id, Organizer updatedOrganizer)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organizer == null)
            {
                return NotFound();
            }

            // Update details
            organizer.User.Name = updatedOrganizer.User.Name;
            organizer.User.Email = updatedOrganizer.User.Email;

            organizer.MobileNumber = updatedOrganizer.MobileNumber;

            await _context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        //change password
        // GET: Organizer/ChangePassword
        public async Task<IActionResult> ChangePassword(int id)
        {
            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organizer == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                OrganizerId = id
            };

            return View(model);
        }

        // POST: Organizer/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var organizer = await _context.Organizers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == model.OrganizerId);

            if (organizer == null)
            {
                return NotFound();
            }

            var hasher = new PasswordHasher<User>();

            // Verify current password
            var verificationResult = hasher.VerifyHashedPassword(organizer.User, organizer.User.Password, model.CurrentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("CurrentPassword", "Incorrect current password.");
                return View(model);
            }

            // Hash and update the new password
            organizer.User.Password = hasher.HashPassword(organizer.User, model.NewPassword);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
        // GET: Organizer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ViewBag.WelcomeMessage = HttpContext.Session.GetString("WelcomeMessage");
            HttpContext.Session.Remove("WelcomeMessage"); // Clear it after reading

            // Get the organizer by userId
            var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
            {
                return NotFound("Organizer not found.");
            }

            // Get tournaments by the organizer
            var tournaments = await _context.Tournaments
     .Include(t => t.Sport) // Include Sport so it's accessible in the view
     .Where(t => t.OrganizerId == organizer.Id)
     .ToListAsync();


            // Get teams for those tournaments
            var teams = await _context.Teams
                .Where(t => t.TournamentId.HasValue && tournaments.Select(tt => tt.Id).Contains(t.TournamentId.Value))
                .ToListAsync();

            var teamIds = teams.Select(t => t.Id).ToList();

            // Get total players for those teams
            var totalPlayers = await _context.TeamPlayers
                .CountAsync(tp => tp.TeamId.HasValue && teamIds.Contains(tp.TeamId.Value));

            // ViewModel for dashboard
            var viewModel = new OrganizerDashboardViewModel
            {
                Tournaments = tournaments,
                Teams = teams,
                TotalPlayers = totalPlayers
            };

            return View(viewModel);
        }

        public IActionResult Tournaments(string searchQuery, int? sportFilter)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get organizer ID based on logged-in user ID
            var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
            if (organizer == null)
            {
                return Unauthorized(); // Or show message if not an organizer
            }

            var tournamentsQuery = _context.Tournaments
                .Include(t => t.Organizer).ThenInclude(o => o.User)
                .Include(t => t.Sport)
                .Include(t => t.BidMasters)
                .Where(t => t.OrganizerId == organizer.Id) // ✅ filter by current organizer
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                tournamentsQuery = tournamentsQuery.Where(t =>
                    t.Name.Contains(searchQuery) ||
                    t.Venue.Contains(searchQuery) ||
                    t.Organizer.User.Name.Contains(searchQuery) ||
                    t.Sport.Name.Contains(searchQuery));
            }

            if (sportFilter.HasValue)
            {
                tournamentsQuery = tournamentsQuery.Where(t => t.SportId == sportFilter.Value);
            }

            var tournamentList = tournamentsQuery
                .Select(t => new TournamentListViewModel
                {
                    Id = t.Id,
                    Logo = t.Logo,
                    Name = t.Name,
                    Date = t.Date,
                    Venue = t.Venue,
                    OrganizerName = t.Organizer.User.Name,
                    SportName = t.Sport.Name,
                    BidStatus = t.BidMasters
                        .OrderByDescending(b => b.Id)
                        .Select(b => b.Status)
                        .FirstOrDefault() ?? "Not Set"
                })
                .ToList();

            ViewBag.Sports = _context.Sports.ToList();
            ViewBag.SelectedSport = sportFilter;

            return View(tournamentList);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTournament()
        {
            var sports = await _context.Sports.ToListAsync();

            var viewModel = new CreateTournamentViewModel
            {
                Sports = sports
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTournament(CreateTournamentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Sports = await _context.Sports.ToListAsync();
                return View(model);
            }

            // Get the logged-in organizer
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
            {
                return NotFound("Organizer not found.");
            }

            string? logoPath = null;

            if (model.LogoFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(stream);
                }

                logoPath = uniqueFileName; // Just "photo.jpg"
                                           // Relative path to access from view
            }

            var tournament = new Tournament
            {
                Name = model.Name,
                Date = model.Date,
                Venue = model.Venue,
                SportId = model.SportId,
                OrganizerId = organizer.Id,
                Logo = logoPath
            };

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            // Insert into BidMaster
            var bidMaster = new BidMaster
            {
                TournamentId = tournament.Id,
                PointsPerTeam = model.PointsPerTeam,
                MinBid = model.MinBid,
                IncreaseOfBid = model.IncreaseOfBid,
                PlayersPerTeam = model.PlayersPerTeam,
                Status = "Upcoming"
            };

            _context.BidMasters.Add(bidMaster);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }


        public async Task<IActionResult> ViewTournament(int? id)
        {
            if (id == null)
                return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Organizer).ThenInclude(o => o.User)
                .Include(t => t.Sport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
                return NotFound();

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

            var bidMaster = await _context.BidMasters
                .FirstOrDefaultAsync(b => b.TournamentId == tournament.Id);

            var viewModel = new TournamentDetailViewModel
            {
                Tournament = tournament,
                TournamentPlayers = tournamentPlayers,
                Teams = teams,
                CaptainNames = captainNames,
                CaptainRoles = captainRoles,
                BidMaster = bidMaster
            };

            return View(viewModel);  // Pass the correct model here
        }

        // GET: Organizer/EditTournament/5
        [HttpGet]
        public async Task<IActionResult> EditTournament(int? id)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Sport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return NotFound();

            var bidMaster = await _context.BidMasters.FirstOrDefaultAsync(b => b.TournamentId == tournament.Id);

            // Check if any tournament is active (status = "Active")
            bool isAnyActiveTournament = await _context.BidMasters
                .AnyAsync(b => b.Status == "Active" && b.TournamentId != tournament.Id);

            var viewModel = new CreateTournamentViewModel
            {
                Name = tournament.Name,
                Venue = tournament.Venue,
                Date = tournament.Date,
                SportId = (int)tournament.SportId, // 👈 Explicit cast here
                ExistingLogoPath = tournament.Logo,

                PointsPerTeam = bidMaster?.PointsPerTeam ?? 0,
                MinBid = bidMaster?.MinBid ?? 0,
                IncreaseOfBid = bidMaster?.IncreaseOfBid ?? 0,
                PlayersPerTeam = bidMaster?.PlayersPerTeam ?? 0,
                BidStatus = bidMaster?.Status,  // Pass the existing BidMaster status

                Sports = await _context.Sports.ToListAsync(),
                IsAnyActiveTournament = isAnyActiveTournament // Flag to show/hide dropdown
            };

            ViewBag.TournamentId = tournament.Id;
            return View(viewModel);
        }

        // POST: Organizer/EditTournament
        [HttpPost]
        public async Task<IActionResult> EditTournament(int id, CreateTournamentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Sports = await _context.Sports.ToListAsync();
                return View(model);
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            string? logoPath = tournament.Logo;

            if (model.LogoFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(stream);
                }

                logoPath = uniqueFileName;
            }

            tournament.Name = model.Name;
            tournament.Date = model.Date;
            tournament.Venue = model.Venue;
            tournament.SportId = model.SportId;
            tournament.Logo = logoPath;

            _context.Tournaments.Update(tournament);

            var bidMaster = await _context.BidMasters.FirstOrDefaultAsync(b => b.TournamentId == tournament.Id);
            if (bidMaster != null)
            {
                bidMaster.PointsPerTeam = model.PointsPerTeam;
                bidMaster.MinBid = model.MinBid;
                bidMaster.IncreaseOfBid = model.IncreaseOfBid;
                bidMaster.PlayersPerTeam = model.PlayersPerTeam;
                bidMaster.Status = string.IsNullOrEmpty(model.BidStatus) ? "Inactive" : model.BidStatus;

                _context.BidMasters.Update(bidMaster);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Tournament updated successfully!";
            return RedirectToAction("Tournaments");
        }


        [HttpGet]
        public async Task<IActionResult> DeleteTournament(int? id)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Sport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return NotFound();

            return View(tournament);
        }

        [HttpPost, ActionName("DeleteTournament")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTournamentConfirmed(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);

            if (tournament == null) return NotFound();

            // Delete related Bidders
            var bidders = _context.Bidders.Where(b => b.TournamentId == id);
            _context.Bidders.RemoveRange(bidders);

            // Delete related TournamentPlayers
            var tPlayers = _context.TournamentPlayers.Where(tp => tp.TournamentId == id);
            _context.TournamentPlayers.RemoveRange(tPlayers);

            // Delete related Teams
            var teams = _context.Teams.Where(t => t.TournamentId == id);
            _context.Teams.RemoveRange(teams);

            // Delete related BidMaster
            var bidMaster = await _context.BidMasters.FirstOrDefaultAsync(b => b.TournamentId == id);
            if (bidMaster != null) _context.BidMasters.Remove(bidMaster);

            // Delete Tournament
            _context.Tournaments.Remove(tournament);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Tournament deleted successfully!";

            return RedirectToAction("Dashboard");
        }


        //add method for player list
        public async Task<IActionResult> TournamentPlayersList(int? id, string statusFilter = null)
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.Sport)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null) return NotFound();

            var tournamentPlayersQuery = _context.TournamentPlayers
                .Include(tp => tp.Player)
                .ThenInclude(p => p.User)
                .Where(tp => tp.TournamentId == tournament.Id);

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                tournamentPlayersQuery = tournamentPlayersQuery.Where(tp => tp.AvailabilityStatus == statusFilter);
            }

            var tournamentPlayers = await tournamentPlayersQuery.ToListAsync();

            return View(tournamentPlayers);
        }

        //manage teams method
        public async Task<IActionResult> ManageTeams()
        {
            // Fetch all teams for all tournaments (or you can fetch for a specific tournament if needed)
            var teams = await _context.Teams
                .Include(t => t.Tournament)   // Include the related Tournament info
                .Include(t => t.Captain)      // Include the related Captain
                .ThenInclude(c => c.Player)   // Include Captain's Player information
                .ToListAsync();

            // Pass the list of teams to the view
            return View(teams);
        }

        public IActionResult AddTeams(int tournamentId)
        {
            ViewBag.TournamentId = tournamentId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTeams(int tournamentId, List<string> teamNames)
        {
            if (teamNames == null || teamNames.Count == 0)
                return BadRequest("No team names provided.");

            var bidMaster = await _context.BidMasters
                .FirstOrDefaultAsync(b => b.TournamentId == tournamentId);

            if (bidMaster == null)
                return NotFound("Bid settings not found.");

            // Get existing team names for the tournament (case-insensitive comparison)
            var existingTeamNames = await _context.Teams
                .Where(t => t.TournamentId == tournamentId)
                .Select(t => t.TeamName.ToLower())
                .ToListAsync();

            var newTeams = new List<Team>();

            foreach (var name in teamNames)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;

                var trimmedName = name.Trim();

                if (existingTeamNames.Contains(trimmedName.ToLower())) continue;

                newTeams.Add(new Team
                {
                    TournamentId = tournamentId,
                    TeamName = trimmedName,
                    Budget = bidMaster.PointsPerTeam
                });
            }

            if (newTeams.Count == 0)
            {
                TempData["Error"] = "No new unique teams were added. All team names might already exist.";
                return RedirectToAction("ViewTournament", new { id = tournamentId });
            }

            _context.Teams.AddRange(newTeams);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{newTeams.Count} teams successfully added.";
            return RedirectToAction("ViewTournament", new { id = tournamentId });
        }

        public async Task<IActionResult> OrganizerTeams(int? tournamentId, int? sportId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organizer == null) return Unauthorized();

            var tournaments = await _context.Tournaments
                .Where(t => t.OrganizerId == organizer.Id)
                .Include(t => t.Sport)
                .ToListAsync();

            var teamsQuery = _context.Teams
                .Include(t => t.Tournament)
                .ThenInclude(t => t.Sport)
                .Where(t => t.Tournament.OrganizerId == organizer.Id)
                .AsQueryable();

            if (tournamentId.HasValue)
                teamsQuery = teamsQuery.Where(t => t.TournamentId == tournamentId.Value);

            if (sportId.HasValue)
                teamsQuery = teamsQuery.Where(t => t.Tournament.SportId == sportId.Value);

            var viewModel = new OrganizerTeamsViewModel
            {
                Teams = await teamsQuery.ToListAsync(),
                Tournaments = tournaments,
                SelectedTournamentId = tournamentId,
                SelectedSportId = sportId,
                Sports = await _context.Sports.ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ViewTeamPlayers(int teamId)
        {
            // Get the tournament ID and sport ID for this team
            var team = await _context.Teams
                .Where(t => t.Id == teamId)
                .Include(t => t.Tournament)
                .ThenInclude(t => t.Sport)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound();
            }

            var sportId = team.Tournament.SportId;

            // Fetch players and their sport category matching the tournament's sport
            var players = await _context.TeamPlayers
                .Where(tp => tp.TeamId == teamId)
                .Include(tp => tp.Player)
                    .ThenInclude(p => p.User)
                .Include(tp => tp.Player)
                    .ThenInclude(p => p.PlayerSports)
                        .ThenInclude(ps => ps.SportCategory)
                .Select(tp => new PlayerWithSportViewModel
                {
                    Name = tp.Player.User.Name,
                    SportCategoryName = tp.Player.PlayerSports
                        .Where(ps => ps.SportCategory.SportId == sportId)
                        .Select(ps => ps.SportCategory.Name)
                        .FirstOrDefault(),
                    PhotoUrl = tp.Player.Photo
                })
                .ToListAsync();

            if (players == null || !players.Any())
            {
                ViewBag.Message = "No auction has been held, so no team players are allotted.";
            }

            return View(players);
        }

        // GET: AssignCaptain
        public async Task<IActionResult> AssignCaptain(int teamId, int tournamentId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound();

            if (team.CaptainId.HasValue)
            {
                TempData["Error"] = "Captain already assigned.";
                return RedirectToAction("OrganizerTeams");
            }

            // Get all players in this tournament
            var tournamentPlayers = await _context.TournamentPlayers
                .Include(tp => tp.Player)
                .ThenInclude(p => p.User)
                .Where(tp => tp.TournamentId == tournamentId && tp.AvailabilityStatus != "SOLD")
                .ToListAsync();

            var viewModel = new AssignCaptainViewModel
            {
                TeamId = teamId,
                TournamentId = tournamentId,
                Players = tournamentPlayers.Select(tp => new PlayerDisplay
                {
                    PlayerId = tp.Player.Id,
                    Name = tp.Player.User.Name,
              
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: AssignCaptain
        [HttpPost]
        public async Task<IActionResult> AssignCaptain(int teamId, int tournamentId, int playerId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null || team.CaptainId.HasValue)
            {
                TempData["Error"] = "Invalid operation.";
                return RedirectToAction("OrganizerTeams");
            }

            // Create Bidder
            var bidder = new Bidder
            {
                PlayerId = playerId,
                TournamentId = tournamentId,
                TeamId = teamId
            };
            _context.Bidders.Add(bidder);
            await _context.SaveChangesAsync();

            // Assign as Captain
            team.CaptainId = bidder.Id;
            _context.Teams.Update(team);

            // Add BidderRole
            _context.BidderRoles.Add(new BidderRole
            {
                BidderId = bidder.Id,
                RoleName = "Captain"
            });

            // Add to TeamPlayers
            _context.TeamPlayers.Add(new TeamPlayer
            {
                PlayerId = playerId,
                TournamentId = tournamentId,
                TeamId = teamId
            });

            // Update Availability
            var tournamentPlayer = await _context.TournamentPlayers
                .FirstOrDefaultAsync(tp => tp.PlayerId == playerId && tp.TournamentId == tournamentId);
            if (tournamentPlayer != null)
            {
                tournamentPlayer.AvailabilityStatus = "SOLD";
                _context.TournamentPlayers.Update(tournamentPlayer);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Captain assigned successfully!";
            return RedirectToAction("OrganizerTeams");
        }

    }
}
