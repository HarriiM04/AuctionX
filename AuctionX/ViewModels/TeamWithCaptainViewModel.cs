using AuctionX.Models;

namespace AuctionX.ViewModels
{
    public class TeamWithCaptainViewModel
    {
        public Team Team { get; set; }
        public string CaptainName { get; set; }
        public List<Player> PlayersInTeam { get; set; }
    }
}
