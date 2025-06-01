using AuctionX.Models;

namespace AuctionX.ViewModels
{
    public class BidderDashboardViewModel
    {
        public string PlayerName { get; set; }
        public string TournamentName { get; set; }
        public string TeamName { get; set; }
        public bool IsBidder { get; set; }
    }

}
