using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class Sport
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public virtual ICollection<SportCategory> SportCategories { get; set; } = new List<SportCategory>();

    public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}
