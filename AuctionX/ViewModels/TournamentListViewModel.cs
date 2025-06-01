namespace AuctionX.ViewModels
{
    public class TournamentListViewModel
    {
        public int Id { get; set; }
        public string Logo { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Venue { get; set; }
        public string OrganizerName { get; set; }
        public string SportName { get; set; }
        public string BidStatus { get; set; } // From BidMaster
    }

}
