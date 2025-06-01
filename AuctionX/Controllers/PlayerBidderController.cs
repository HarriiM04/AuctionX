using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuctionX.Models;
using AuctionX.ViewModels;

namespace AuctionX.Controllers
{
    public class PlayerBidderController : Controller
    {
        private readonly SportsAuctionContext _context;

        public PlayerBidderController(SportsAuctionContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Dashboard()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ensure the userId is valid
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get player by userId
            var player = await _context.Players
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (player == null)
            {
                return NotFound("Player not found.");
            }

            // Get bidder info for that player
            var bidder = await _context.Bidders
                .Include(b => b.Team)
                .Include(b => b.Tournament)
                .FirstOrDefaultAsync(b => b.PlayerId == player.Id);

            var viewModel = new PlayerBidderViewModel
            {
                Player = player,
                Bidder = bidder
            };

            return View(viewModel);
        }

    }
}
