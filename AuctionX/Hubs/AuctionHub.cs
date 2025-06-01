using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuctionX.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace AuctionX.Hubs
{
    public class AuctionHub : Hub
    {
        private static Queue<Player> playerQueue = new();
        private static Player? currentPlayer = null;
        private static int? currentBid = null;
        private static string? currentTeam = null;
        private static CancellationTokenSource? cts;
        private static bool timerRunning = false;
        private static bool isPaused = false;

        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly SportsAuctionContext _context;

        public AuctionHub(IHubContext<AuctionHub> hubContext, SportsAuctionContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }


        public Task InitializeQueue(List<Player> players)
        {
            playerQueue = new Queue<Player>(players);
            currentPlayer = null;
            currentBid = null;
            currentTeam = null;
            timerRunning = false;
            isPaused = true; // Wait for organizer to start
            return Task.CompletedTask;
        }

        public async Task StartAuction(int tournamentId)
        {
            isPaused = false;

            if (currentPlayer == null)
            {
                await NextPlayer(); // Auto-start from first player
            }
            else if (!timerRunning)
            {
                // Resume current player if already set
                cts?.Cancel();
                cts = new CancellationTokenSource();
                var token = cts.Token;
                _ = StartBiddingTimer(token);
            }

          

            // Broadcast team player counts and player stats after starting
            await BroadcastTeamPlayerCounts(tournamentId);
            await BroadcastPlayerStats(tournamentId);

        }

        public async Task PauseAuction()
        {
            isPaused = true;
            timerRunning = false;
            cts?.Cancel();
            await _hubContext.Clients.All.SendAsync("TimerPaused");
        }

        private async Task NextPlayer()
        {
            if (playerQueue.Any())
            {
                currentPlayer = playerQueue.Dequeue();
                currentBid = 0;
                currentTeam = null;
                timerRunning = false;

                await _hubContext.Clients.All.SendAsync("NewPlayerForBidding", currentPlayer);
                await _hubContext.Clients.All.SendAsync("TimerPaused");

                cts?.Cancel();
                cts = new CancellationTokenSource();

                if (!isPaused)
                {
                    var token = cts.Token;
                    _ = StartBiddingTimer(token);
                }
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("AuctionEnded");
            }
        }

        public async Task SendBid(string teamName, int bidAmount)
        {
            if (currentPlayer == null) return;

            if (bidAmount > (currentBid ?? 0))
            {
                // Get the team and bidder details
                var team = await _context.Teams
                    .Include(t => t.Captain)
                    .FirstOrDefaultAsync(t => t.TeamName == teamName);

                if (team != null && team.Captain != null)
                {
                    // Store the bid in Bids table
                    var bid = new Bid
                    {
                        BidderId = team.Captain.Id,
                        PlayerId = currentPlayer.Id,
                        BidAmount = bidAmount,
                        BidTime = DateTime.UtcNow
                    };
                    _context.Bids.Add(bid);
                    await _context.SaveChangesAsync();
                }

                currentBid = bidAmount;
                currentTeam = teamName;

                await _hubContext.Clients.All.SendAsync("ReceiveBid", teamName, bidAmount, currentPlayer?.Id);

                if (timerRunning)
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    var token = cts.Token;
                    _ = StartBiddingTimer(token);
                }
            }
        }

        public async Task FinalizeCurrentBid()
        {
            cts?.Cancel();
            timerRunning = false;

            if (currentPlayer != null)
            {
                var activeBid = await _context.BidMasters
                    .Include(b => b.Tournament)
                    .FirstOrDefaultAsync(b => b.Status == "Active");

                if (activeBid?.TournamentId == null) return;

                var tournamentId = activeBid.TournamentId.Value;

                if (currentBid.HasValue && currentBid > 0 && currentTeam != null)
                {
                    await _hubContext.Clients.All.SendAsync("PlayerSold", currentPlayer.Id, currentTeam, currentBid);

                    var team = await _context.Teams
                        .Include(t => t.Captain)
                        .FirstOrDefaultAsync(t => t.TeamName == currentTeam);

                    if (team != null)
                    {
                        _context.TournamentResults.Add(new TournamentResult
                        {
                            TournamentId = tournamentId,
                            PlayerId = currentPlayer.Id,
                            TeamId = team.Id,
                            FinalPrice = currentBid.Value,
                            AssignedTime = DateTime.UtcNow,
                            Status = "SOLD"
                        });

                        _context.TeamPlayers.Add(new TeamPlayer
                        {
                            TeamId = team.Id,
                            PlayerId = currentPlayer.Id,
                            TournamentId = tournamentId
                        });

                        var tournamentPlayer = await _context.TournamentPlayers
                            .FirstOrDefaultAsync(tp => tp.PlayerId == currentPlayer.Id && tp.TournamentId == tournamentId);

                        if (tournamentPlayer != null)
                        {
                            tournamentPlayer.AvailabilityStatus = "SOLD";
                        }

                        team.Budget -= currentBid.Value;
                        await _context.SaveChangesAsync();
                        await BroadcastTeamPlayerCounts(tournamentId); // after SaveChangesAsync()
                        await BroadcastPlayerStats(tournamentId);


                    }
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("PlayerUnsold", currentPlayer.Id);

                    var tournamentPlayer = await _context.TournamentPlayers
                        .FirstOrDefaultAsync(tp => tp.PlayerId == currentPlayer.Id && tp.TournamentId == tournamentId);

                    if (tournamentPlayer != null)
                    {
                        tournamentPlayer.AvailabilityStatus = "UNSOLD";

                        _context.TournamentResults.Add(new TournamentResult
                        {
                            TournamentId = tournamentId,
                            PlayerId = currentPlayer.Id,
                            TeamId = null, // No team assigned
                            FinalPrice = 0,
                            AssignedTime = DateTime.UtcNow,
                            Status = "UNSOLD"
                        });


                        await _context.SaveChangesAsync();
                        await BroadcastTeamPlayerCounts(tournamentId); // after SaveChangesAsync()
                        await BroadcastPlayerStats(tournamentId);
                    }
                }


                // Delay before moving to next player (5 to 7 seconds)
                for (int i = 10; i >= 1; i--)
                {
                    await _hubContext.Clients.All.SendAsync("NextPlayerCountdown", i);
                    await Task.Delay(1000);
                }

                await NextPlayer(); // Automatically go to next player
            }
        }


        private async Task StartBiddingTimer(CancellationToken token)
        {
            try
            {
                timerRunning = true;
                int secondsLeft = 15;
                while (secondsLeft >= 0 && !token.IsCancellationRequested)

                {
                    await _hubContext.Clients.All.SendAsync("TimerUpdate", secondsLeft);
                    await Task.Delay(1000, token);
                    secondsLeft--;
                }

                if (!token.IsCancellationRequested)
                {
                    await FinalizeCurrentBid();
                }
            }
            catch (TaskCanceledException)
            {
                // Timer manually stopped or reset
            }
        }

        private async Task BroadcastTeamPlayerCounts(int tournamentId)
        {
            var teamPlayerCounts = await _context.TeamPlayers
                .Where(tp => tp.TournamentId == tournamentId)
                .GroupBy(tp => tp.TeamId)
                .Select(g => new
                {
                    TeamId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            await _hubContext.Clients.All.SendAsync("UpdateTeamPlayerCounts", teamPlayerCounts);
        }

        private async Task BroadcastPlayerStats(int tournamentId)
        {
            var totalPlayers = await _context.TournamentPlayers
                .Where(tp => tp.TournamentId == tournamentId)
                .CountAsync();

            var unsoldPlayers = await _context.TournamentPlayers
                .Where(tp => tp.TournamentId == tournamentId && tp.AvailabilityStatus != "SOLD")
                .CountAsync();

            await _hubContext.Clients.All.SendAsync("UpdatePlayerStats", totalPlayers, unsoldPlayers);
        }


    }
}
