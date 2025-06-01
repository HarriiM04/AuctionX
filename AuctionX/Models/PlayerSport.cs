using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class PlayerSport
{
    public int Id { get; set; }

    public int? PlayerId { get; set; }

    public int? SportCategoryId { get; set; }

    public virtual Player? Player { get; set; }

    public virtual SportCategory? SportCategory { get; set; }
}
