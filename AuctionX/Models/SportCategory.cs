using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class SportCategory
{
    public int Id { get; set; }

    public int? SportId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PlayerSport> PlayerSports { get; set; } = new List<PlayerSport>();

    public virtual Sport? Sport { get; set; }
}
