using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AuctionX.Models;

public partial class SportsAuctionContext : DbContext
{
    public SportsAuctionContext()
    {
    }

    public SportsAuctionContext(DbContextOptions<SportsAuctionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Bid> Bids { get; set; }

    public virtual DbSet<BidMaster> BidMasters { get; set; }

    public virtual DbSet<Bidder> Bidders { get; set; }

    public virtual DbSet<BidderRole> BidderRoles { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Organizer> Organizers { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerSport> PlayerSports { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    public virtual DbSet<SportCategory> SportCategories { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamPlayer> TeamPlayers { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<TournamentPlayer> TournamentPlayers { get; set; }

    public virtual DbSet<TournamentResult> TournamentResults { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SportsAuction;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0729E727E2");

            entity.HasOne(d => d.User).WithMany(p => p.Admins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Admins__UserId__0880433F");
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bids__3214EC0733A970BC");

            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidTime).HasColumnType("datetime");

            entity.HasOne(d => d.Bidder).WithMany(p => p.Bids)
                .HasForeignKey(d => d.BidderId)
                .HasConstraintName("FK__Bids__BidderId__37703C52");

            entity.HasOne(d => d.Player).WithMany(p => p.Bids)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_Bids_PlayerId");
        });

        modelBuilder.Entity<BidMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BidMaste__3214EC07A15C6CCA");

            entity.ToTable("BidMaster");

            entity.Property(e => e.IncreaseOfBid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinBid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Tournament).WithMany(p => p.BidMasters)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("FK__BidMaster__Tourn__25518C17");
        });

        modelBuilder.Entity<Bidder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bidders__3214EC075A18E370");

            entity.HasOne(d => d.Player).WithMany(p => p.Bidders)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_Bidders_PlayerId");

            entity.HasOne(d => d.Team).WithMany(p => p.Bidders)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK_Bidders_TeamId");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Bidders)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("FK__Bidders__Tournam__2FCF1A8A");
        });

        modelBuilder.Entity<BidderRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BidderRo__3214EC0701FD60F0");

            entity.Property(e => e.RoleName).HasMaxLength(255);

            entity.HasOne(d => d.Bidder).WithMany(p => p.BidderRoles)
                .HasForeignKey(d => d.BidderId)
                .HasConstraintName("FK__BidderRol__Bidde__3493CFA7");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC072840E6B5");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__44CA3770");
        });

        modelBuilder.Entity<Organizer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC078A7CEAB8");

            entity.Property(e => e.MobileNumber).HasMaxLength(15);

            entity.HasOne(d => d.User).WithMany(p => p.Organizers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Organizer__UserI__0B5CAFEA");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC07B5684371");

            entity.Property(e => e.AvailabilityStatus).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MobileNumber).HasMaxLength(15);
            entity.Property(e => e.Photo).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Players)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Players__UserId__14E61A24");
        });

        modelBuilder.Entity<PlayerSport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlayerSp__3214EC077F6FD2F7");

            entity.HasOne(d => d.Player).WithMany(p => p.PlayerSports)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_PlayerSports_PlayerId");

            entity.HasOne(d => d.SportCategory).WithMany(p => p.PlayerSports)
                .HasForeignKey(d => d.SportCategoryId)
                .HasConstraintName("FK__PlayerSpo__Sport__1EA48E88");
        });

        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sports__3214EC078C2A8B89");

            entity.Property(e => e.Category).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<SportCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SportCat__3214EC07FD10455D");

            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Sport).WithMany(p => p.SportCategories)
                .HasForeignKey(d => d.SportId)
                .HasConstraintName("FK__SportCate__Sport__1AD3FDA4");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Teams__3214EC07B5930298");

            entity.Property(e => e.Budget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TeamName).HasMaxLength(255);

            entity.HasOne(d => d.Captain).WithMany(p => p.Teams)
                .HasForeignKey(d => d.CaptainId)
                .HasConstraintName("FK_Teams_CaptainId");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Teams)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("FK__Teams__Tournamen__2BFE89A6");
        });

        modelBuilder.Entity<TeamPlayer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamPlay__3214EC07F8784A51");

            entity.HasOne(d => d.Player).WithMany(p => p.TeamPlayers)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_TeamPlayers_PlayerId");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamPlayers)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK__TeamPlaye__TeamI__3B40CD36");

            entity.HasOne(d => d.Tournament).WithMany(p => p.TeamPlayers)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("FK__TeamPlaye__Tourn__3D2915A8");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tourname__3214EC07BD18C433");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Logo).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Venue).HasMaxLength(255);

            entity.HasOne(d => d.Organizer).WithMany(p => p.Tournaments)
                .HasForeignKey(d => d.OrganizerId)
                .HasConstraintName("FK__Tournamen__Organ__0C50D423");

            entity.HasOne(d => d.Sport).WithMany(p => p.Tournaments)
                .HasForeignKey(d => d.SportId)
                .HasConstraintName("FK__Tournamen__Sport__22751F6C");
        });

        modelBuilder.Entity<TournamentPlayer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tourname__3214EC07A9FD6009");

            entity.Property(e => e.AvailabilityStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Unsold");

            entity.HasOne(d => d.Player).WithMany(p => p.TournamentPlayers)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_TournamentPlayers_PlayerId");

            entity.HasOne(d => d.Tournament).WithMany(p => p.TournamentPlayers)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("FK__Tournamen__Tourn__29221CFB");
        });

        modelBuilder.Entity<TournamentResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tourname__3214EC078CE55527");

            entity.Property(e => e.AssignedTime).HasColumnType("datetime");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Player).WithMany(p => p.TournamentResults)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_TournamentResults_PlayerId");

            entity.HasOne(d => d.Team).WithMany(p => p.TournamentResults)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK__Tournamen__TeamI__41EDCAC5");

            entity.HasOne(d => d.Tournament).WithMany(p => p.TournamentResults)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("FK__Tournamen__Tourn__40058253");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07256E1476");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105344394E018").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
