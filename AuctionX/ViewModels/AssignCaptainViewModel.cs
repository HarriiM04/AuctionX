using AuctionX.Models;

namespace AuctionX.ViewModels
{

    public class AssignCaptainViewModel
    {
        public int TeamId { get; set; }
        public int TournamentId { get; set; }
        public List<PlayerDisplay> Players { get; set; }
    }

    public class PlayerDisplay
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
    }
}
