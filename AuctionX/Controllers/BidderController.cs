
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuctionX.Models;
using AuctionX.ViewModels;

namespace AuctionX.Controllers
{
    public class BidderController : Controller
    {
        private readonly SportsAuctionContext _context;

        public BidderController(SportsAuctionContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Dashboard()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve player along with the tournament and team information
            var playerInfo = await _context.Players
                .Include(p => p.User)
                .Join(_context.Bidders,
                      player => player.Id,
                      bidder => bidder.PlayerId,
                      (player, bidder) => new { player, bidder })
                .Join(_context.Teams,
                      combined => combined.bidder.TeamId,
                      team => team.Id,
                      (combined, team) => new { combined.player, combined.bidder, team })
                .Join(_context.Tournaments,
                      combined => combined.bidder.TournamentId,
                      tournament => tournament.Id,
                      (combined, tournament) => new { combined.player, combined.bidder, combined.team, tournament })
                .Select(result => new
                {
                    PlayerName = result.player.User.Name,
                    TournamentName = result.tournament.Name,
                    TeamName = result.team.TeamName
                })
                .FirstOrDefaultAsync();

            if (playerInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if the player is a bidder
            var isBidder = await _context.Bidders.AnyAsync(b => b.PlayerId == userId);

            // Pass navbar flag to layout
            ViewData["Navbar"] = "Captain";

            var viewModel = new BidderDashboardViewModel
            {
                PlayerName = playerInfo.PlayerName,
                TournamentName = playerInfo.TournamentName,
                TeamName = playerInfo.TeamName,
                IsBidder = isBidder
            };

            return View(viewModel);
        }


        public async Task<IActionResult> TeamManagement()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
                return RedirectToAction("Login", "Account");

            // get the Player record
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null) return RedirectToAction("Login", "Account");

            // get this player’s Bidder record
            var bidder = await _context.Bidders
                .FirstOrDefaultAsync(b => b.PlayerId == player.Id);
            if (bidder == null) return RedirectToAction("Login", "Account");

            // load the team + tournament + teamplayers → player → user
            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.TeamPlayers!)
                    .ThenInclude(tp => tp.Player!)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.CaptainId == bidder.Id);

            if (team == null)
                return View("NoTeam");

            // push to ViewData
            ViewData["TeamName"] = team.TeamName;
            ViewData["TournamentName"] = team.Tournament?.Name ?? "Unknown";

            // extract just the player names
            var names = team.TeamPlayers
                .Where(tp => tp.Player?.User != null)
                .Select(tp => tp.Player!.User.Name)
                .ToList();
            ViewData["TeamPlayers"] = names;

            return View();
        }




    }
}


