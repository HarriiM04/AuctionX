using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class BidMaster
{
    public int Id { get; set; }

    public int? TournamentId { get; set; }

    public int PointsPerTeam { get; set; }

    public decimal MinBid { get; set; }

    public decimal IncreaseOfBid { get; set; }

    public int PlayersPerTeam { get; set; }

    public string Status { get; set; } = null!;

    public virtual Tournament? Tournament { get; set; }
}
