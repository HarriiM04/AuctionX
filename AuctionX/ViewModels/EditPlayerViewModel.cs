using Microsoft.AspNetCore.Mvc.Rendering;
using AuctionX.Models;

namespace AuctionX.ViewModels
{
    public class EditPlayerViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Description { get; set; }
        public string AvailabilityStatus { get; set; }

        public List<int> SelectedSports { get; set; }
        public List<SportCategory> SportCategoryList { get; set; }
    }
}
