using AuctionX.Models;

namespace AuctionX.ViewModels
{
    // TournamentDetailViewModel.cs

    public class TournamentDetailViewModel
    {
        public Tournament Tournament { get; set; }
        public List<TournamentPlayer> TournamentPlayers { get; set; }
        public List<Team> Teams { get; set; }
        public Dictionary<int, string> CaptainNames { get; set; } // TeamId => Captain Name
        public Dictionary<int, List<string>> CaptainRoles { get; set; } // TeamId => Roles

        public BidMaster BidMaster { get; set; }

        public bool AlreadyRegistered { get; set; }

        public bool IsBidder { get; set; }
    }




}
