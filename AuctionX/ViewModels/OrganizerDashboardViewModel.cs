using AuctionX.Models;

namespace AuctionX.ViewModels
{
    public class OrganizerDashboardViewModel
    {
        // Add a list of tournaments related to the organizer
        public List<Tournament> Tournaments { get; set; } = new List<Tournament>();

        // Add a list of teams for the tournaments
        public List<Team> Teams { get; set; } = new List<Team>();

        // Total players count from the teams
        public int TotalPlayers { get; set; }
    }

}
