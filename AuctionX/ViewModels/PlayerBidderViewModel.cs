using AuctionX.Models;
namespace AuctionX.ViewModels
{
    public class PlayerBidderViewModel
    {
        public Player Player { get; set; }
        public Bidder Bidder { get; set; }
        public bool IsBidder => Bidder != null; // Checks if Bidder is assigned
    }

}