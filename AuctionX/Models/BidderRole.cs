using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class BidderRole
{
    public int Id { get; set; }

    public int? BidderId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual Bidder? Bidder { get; set; }
}
