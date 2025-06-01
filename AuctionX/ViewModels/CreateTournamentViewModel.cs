using AuctionX.Models;
using System.ComponentModel.DataAnnotations;


namespace AuctionX.ViewModels
{
    public class CreateTournamentViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Venue { get; set; }

        [Required]
        public int SportId { get; set; }

        public IFormFile? LogoFile { get; set; }
        public string? ExistingLogoPath { get; set; }

        // BidMaster Fields
        [Required]
        public int PointsPerTeam { get; set; }

        [Required]
        public decimal MinBid { get; set; }

        [Required]
        public decimal IncreaseOfBid { get; set; }

        [Required]
        public int PlayersPerTeam { get; set; }

        public List<Sport>? Sports { get; set; }

        public string? BidStatus { get; set; } // BidMaster status (Active/Inactive/Upcoming)

        public bool IsAnyActiveTournament { get; set; }
    }

}
