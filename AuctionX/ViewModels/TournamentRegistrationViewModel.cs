public class TournamentRegistrationViewModel
{
    public int TournamentId { get; set; }
    public string TournamentName { get; set; }
    public string SportName { get; set; }
    public DateTime TournamentDate { get; set; }
    public string Venue { get; set; }
    public string AvailabilityStatus { get; set; }
    public bool IsBidder { get; set; } // Ensure this property exists
}
