using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class Bidder
{
    public int Id { get; set; }

    public int? PlayerId { get; set; }

    public int? TournamentId { get; set; }

    public int? TeamId { get; set; }

    public virtual ICollection<BidderRole> BidderRoles { get; set; } = new List<BidderRole>();

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual Player? Player { get; set; }

    public virtual Team? Team { get; set; }

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual Tournament? Tournament { get; set; }
}
