using AuctionX.Models;
namespace AuctionX.ViewModels
{
    public class PlayerDashboardViewModel
    {
        public Player Player { get; set; }
        public bool IsBidder { get; set; }
        public List<Tournament> Tournaments { get; set; } // List of tournaments to display
        public List<Sport> Sports { get; set; } // List of sports for filtering
        public int? SelectedSportId { get; set; } // To store the selected sport filter
        public string SearchName { get; set; } // To store the tournament name filter
    }
}
