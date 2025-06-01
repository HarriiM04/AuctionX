using AuctionX.Models;

namespace AuctionX.ViewModels
{
    public class OrganizerTeamsViewModel
    {
        public List<Team> Teams { get; set; }
        public List<Tournament> Tournaments { get; set; }
        public int? SelectedTournamentId { get; set; }
        public int? SelectedSportId { get; set; }
        public List<Sport> Sports { get; set; }
    }

}
