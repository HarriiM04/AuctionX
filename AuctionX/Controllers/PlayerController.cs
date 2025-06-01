
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuctionX.Models;
using AuctionX.ViewModels;

namespace AuctionX.Controllers
{

    public class PlayerController : Controller
    {
        private readonly SportsAuctionContext _context;

        public PlayerController(SportsAuctionContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard(string search)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var player = await _context.Players
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (player == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var isBidder = await _context.Bidders.AnyAsync(b => b.PlayerId == player.Id);

            var tournamentsQuery = _context.Tournaments
                .Include(t => t.Sport)
                .Include(t => t.BidMasters)
                .Where(t => t.BidMasters.Any(bm => bm.Status == "Active" || bm.Status == "Inactive"))
                .AsQueryable();

            // Apply single search across multiple fields
            if (!string.IsNullOrEmpty(search))
            {
                tournamentsQuery = tournamentsQuery.Where(t =>
                    t.Name.Contains(search) ||
                    t.BidMasters.Any(bm => bm.Status.Contains(search)) ||
                    (t.Sport != null && t.Sport.Name.Contains(search))
                );
            }

            var tournamentList = await tournamentsQuery
                .Select(t => new
                {
                    Id = t.Id,
                    TournamentName = t.Name,
                    SportName = t.Sport != null ? t.Sport.Name : "",
                    Venue = t.Venue,
                    Status = t.BidMasters.FirstOrDefault().Status
                })
                .ToListAsync();

            ViewBag.Tournaments = tournamentList;
            ViewBag.Search = search;

            var viewModel = new PlayerDashboardViewModel
            {
                Player = player,
                IsBidder = isBidder
            };

            return View(viewModel);
        }




        // GET: Player/TournamentDetails/5
        [Route("Player/TournamentDetails/{id}")]
        public async Task<IActionResult> TournamentDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int uid))
                return Challenge(); // or redirect to login

            // Find the current player
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == uid);
            if (player == null)
                return RedirectToAction("Login", "Account");

            // Check registration & bidder status
            ViewBag.IsAlreadyRegistered = await _context.TournamentPlayers
                .AnyAsync(tp => tp.PlayerId == player.Id && tp.TournamentId == id);
            ViewBag.IsBidder = await _context.Bidders
                .AnyAsync(b => b.PlayerId == player.Id && b.TournamentId == id);

            // Load all needed tournament data
            var tournament = await _context.Tournaments
                .Include(t => t.Sport)
                .Include(t => t.Organizer)
                    .ThenInclude(o => o.User)
                .Include(t => t.BidMasters)
                .Include(t => t.Teams)
                    .ThenInclude(tm => tm.Captain)
                        .ThenInclude(c => c.Player)
                            .ThenInclude(pl => pl.User)
                .Include(t => t.Bidders)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
                return NotFound();

            return View(tournament);
        }



        // GET: Player/MyTournament
        public async Task<IActionResult> MyTournament(int? sportId)
        {
            // 1) Get current player
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Account");

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
                return RedirectToAction("Login", "Account");

            // 2) Load all sports for the dropdown
            var sportsList = await _context.Sports
                .OrderBy(s => s.Name)
                .ToListAsync();
            ViewBag.Sports = new SelectList(sportsList, "Id", "Name", sportId);

            // 3) Build the base query for this player's registrations
            IQueryable<TournamentPlayer> query = _context.TournamentPlayers
                .Where(tp => tp.PlayerId == player.Id)
                .Include(tp => tp.Tournament)
                    .ThenInclude(t => t.Sport)
                .Include(tp => tp.Tournament)
                    .ThenInclude(t => t.Organizer)
                        .ThenInclude(o => o.User);

            // 4) Apply sport filter if supplied
            if (sportId.HasValue)
            {
                query = query.Where(tp => tp.Tournament.SportId == sportId.Value);
            }

            // 5) Execute
            var registrations = await query.ToListAsync();

            return View(registrations);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForTournament(int tournamentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["RegisterMessage"] = "Login required to register.";
                return RedirectToAction("Login", "Account");
            }

            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
            {
                TempData["RegisterMessage"] = "Player not found.";
                return RedirectToAction("Login", "Account");
            }

            bool alreadyRegistered = await _context.TournamentPlayers
                .AnyAsync(tp => tp.PlayerId == player.Id && tp.TournamentId == tournamentId);

            if (!alreadyRegistered)
            {
                var registration = new TournamentPlayer
                {
                    PlayerId = player.Id,
                    TournamentId = tournamentId,
                    AvailabilityStatus = "UNSOLD"
                };

                _context.TournamentPlayers.Add(registration);
                await _context.SaveChangesAsync();

                TempData["RegisterMessage"] = "You have been registered for this tournament.";
            }
            else
            {
                TempData["RegisterMessage"] = "You are already registered for this tournament.";
            }

            return RedirectToAction("TournamentDetails", new { id = tournamentId });
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Account");

            var player = await _context.Players
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (player == null)
                return NotFound();

            return View(player);  // This will send the player model to the view
        }




        // POST: Player/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Photo,MobileNumber,Description,AvailabilityStatus")] Player player)
        {
            if (id != player.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", player.UserId);
            return View(player);
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.Id == id);
        }

        public IActionResult LiveAuction(int tournamentId)
        {
            ViewBag.TournamentId = tournamentId;
            return View();
        }
    }
}


