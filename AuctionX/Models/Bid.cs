using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class Bid
{
    public int Id { get; set; }

    public int? BidderId { get; set; }

    public int? PlayerId { get; set; }

    public decimal BidAmount { get; set; }

    public DateTime BidTime { get; set; }

    public virtual Bidder? Bidder { get; set; }

    public virtual Player? Player { get; set; }
}
