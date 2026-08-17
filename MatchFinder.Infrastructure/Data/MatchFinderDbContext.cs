using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MatchFinder.Domain.Entities;

// 1. ADDED THE NAMESPACE BACK
namespace MatchFinder.Infrastructure.Data;

public class MatchFinderDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public MatchFinderDbContext(DbContextOptions<MatchFinderDbContext> options) : base(options) { }

    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchPlayer> MatchPlayers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 2. CRITICAL: This generates the AspNetUsers tables!
        base.OnModelCreating(builder);

        // 3. CRITICAL: Sets up the Primary Keys and Relationships
        builder.Entity<MatchPlayer>()
            .HasKey(mp => new { mp.MatchId, mp.UserId });

        builder.Entity<MatchPlayer>()
            .HasOne(mp => mp.Match)
            .WithMany(m => m.MatchPlayers)
            .HasForeignKey(mp => mp.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MatchPlayer>()
            .HasOne(mp => mp.User)
            .WithMany(u => u.MatchPlayers)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Match>()
            .HasOne(m => m.Host)
            .WithMany(u => u.HostedMatches)
            .HasForeignKey(m => m.HostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}