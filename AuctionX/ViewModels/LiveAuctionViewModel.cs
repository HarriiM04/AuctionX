namespace AuctionX.ViewModels;

using AuctionX.Models;

public class LiveAuctionViewModel
{
    public Tournament Tournament { get; set; }
    public BidMaster BidSettings { get; set; }

    // List of players in the tournament
    public List<Player> TournamentPlayers { get; set; }

    public List<Team> Teams { get; set; }

    public Player? CurrentBidPlayer { get; set; }
    public int? CurrentHighestBid { get; set; }
    public string? HighestBidTeamName { get; set; }

    // ✅ Add user info
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserRole { get; set; }
    public bool IsBidder { get; set; }

    public string? BidderTeamName { get; set; } // ✅ New

    public string OrganizerEmail { get; set; }

    public int TotalPlayersCount { get; set; }
    public int UnsoldPlayersCount { get; set; }


}
