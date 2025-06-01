namespace AuctionX.ViewModels
{
    public class TeamDetailViewModel
    {
        public string TeamName { get; set; }
        public decimal Budget { get; set; }
        public string CaptainName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
