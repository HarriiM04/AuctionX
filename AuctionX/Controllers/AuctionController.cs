using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using AuctionX.Models;
using AuctionX.ViewModels;


[Authorize]
public class AuctionController : Controller
{
    private readonly SportsAuctionContext _context;

    public AuctionController(SportsAuctionContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> LiveAuction()
    {
        // 🔐 Step 1: Authorize user and fetch claims
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToAction("Login", "Account");

        ViewBag.UserName = user.Name;
        ViewBag.UserEmail = user.Email;
        ViewBag.UserRole = user.Role;

        // 🔍 Step 2: Load extended role details
        switch (user.Role)
        {
            case "Admin":
                var admin = await _context.Admins.Include(a => a.User).FirstOrDefaultAsync(a => a.UserId == userId);
                if (admin == null) return Unauthorized("Admin details not found.");
                break;

            case "Organizer":
                var organizer = await _context.Organizers.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == userId);
                if (organizer == null) return Unauthorized("Organizer details not found.");
                break;

            case "Player":
                var player = await _context.Players.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);
                if (player == null) return Unauthorized("Player details not found.");

                // fetching bidder details from bidder and team table 
                // with this i can fetch all details of both the tables

                var bidder = await _context.Bidders
                    .Include(b => b.Team)
                    .FirstOrDefaultAsync(b => b.PlayerId == player.Id);

                if (bidder != null)
                {
                    ViewBag.IsBidder = true;
                    ViewBag.BidderId = bidder.Id;
                    ViewBag.BidderTeamName = bidder.Team?.TeamName; // ✅ Add this
                }

                break;


            default:
                return Unauthorized("Unknown role.");
        }

        // ✅ Step 3: Continue with your existing LiveAuction logic

        // this will fetch all the details from BidMaster, Tournament and Sport Table whose Tournament status is "active"

        var activeBid = await _context.BidMasters
            .Include(b => b.Tournament)
                .ThenInclude(t => t.Sport)
            .FirstOrDefaultAsync(b => b.Status == "Active");

        if (activeBid == null || activeBid.Tournament == null)
        {
            return View("NoActiveTournament");
        }


        // here we can access Teams details with Bidder, Player and User table 

        var teams = await _context.Teams
            .Where(t => t.TournamentId == activeBid.TournamentId)
            .Include(t => t.Captain)
                .ThenInclude(b => b.Player)
                    .ThenInclude(p => p.User)
            .ToListAsync();

        // to fetch bidder id's

        var bidderPlayerIds = teams
            .Where(t => t.Captain?.PlayerId != null)
            .Select(t => t.Captain!.PlayerId)
            .ToList();

        // to fetch player details excluding bidders 

        var players = await _context.TournamentPlayers
    .Where(tp => tp.TournamentId == activeBid.TournamentId
              && tp.AvailabilityStatus == "UNSOLD") // 👈 Only unsold
    .Include(tp => tp.Player)
        .ThenInclude(p => p.User)
    .Where(tp => !bidderPlayerIds.Contains(tp.PlayerId ?? 0)) // 👈 Exclude bidders
    .Select(tp => tp.Player!)
    .ToListAsync();


        ViewBag.ExcludedBidders = bidderPlayerIds;

        var tournamentOrganizer = await _context.Organizers
    .Include(o => o.User) // Include the related User entity to get the email
    .FirstOrDefaultAsync(o => o.Id == activeBid.Tournament.OrganizerId);

        if (tournamentOrganizer == null || tournamentOrganizer.User == null)
        {
            return Unauthorized("Organizer or organizer's user not found.");
        }


        // let me tell what i need now 
        /*
         * i want details of Tournament and BidMaster Table
         * Teams table
         * Players table 
         * Sports table
         * TournamentPlayers table 
         * Bidder table
         * Bids table
         * 
         */

        // ✅ Fetch total players in the tournament
        var totalPlayersCount = await _context.TournamentPlayers
            .Where(tp => tp.TournamentId == activeBid.TournamentId)
            .CountAsync();

        // ✅ Fetch players whose AvailabilityStatus is "UNSOLD"
        var unsoldPlayersCount = await _context.TournamentPlayers
            .Where(tp => tp.TournamentId == activeBid.TournamentId && tp.AvailabilityStatus == "UNSOLD")
            .CountAsync();

        // Optionally: Pass them to the ViewBag
        ViewBag.TotalPlayersCount = totalPlayersCount;
        ViewBag.UnsoldPlayersCount = unsoldPlayersCount;


        var viewModel = new LiveAuctionViewModel
        {
            Tournament = activeBid.Tournament,
            BidSettings = activeBid,
            TournamentPlayers = players,
            Teams = teams,
            UserName = user.Name,
            UserEmail = user.Email,
            UserRole = user.Role,
            IsBidder = ViewBag.IsBidder ?? false,
            BidderTeamName = ViewBag.BidderTeamName,
            OrganizerEmail = tournamentOrganizer.User.Email,
            TotalPlayersCount = totalPlayersCount,
            UnsoldPlayersCount = unsoldPlayersCount
        };



        return View(viewModel);
    }

}
