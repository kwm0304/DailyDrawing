using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Entities;

namespace Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
  public DbSet<Drawing> Drawings { get; set; }
  public DbSet<Comment> Comments { get; set; }
  public DbSet<Upvote> Upvotes { get; set; }
  public DbSet<Badge> Badges { get; set; }
  public DbSet<Follow> Follows { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }
  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    builder.Entity<AppUser>(e =>
    {
      e.Property(u => u.DisplayName).IsRequired();
      e.Property(u => u.ExperienceLevel).HasConversion<string>();
    });

    builder.Entity<Drawing>()
      .HasOne(d => d.User)
      .WithMany(u => u.Uploads)
      .HasForeignKey(d => d.UserId);

    builder.Entity<Comment>()
      .HasOne(c => c.Drawing)
      .WithMany(d => d.Comments)
      .HasForeignKey(c => c.DrawingId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Comment>()
      .HasOne(c => c.ParentComment)
      .WithMany(c => c.Replies)
      .HasForeignKey(c => c.ParentCommentId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Upvote>()
    .HasKey(du => new { du.AppUserId, du.DrawingId });

    builder.Entity<Badge>()
    .HasOne(b => b.Drawing)
    .WithMany(d => d.Badges)
    .HasForeignKey(b => b.DrawingId)
    .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Follow>(entity =>
    {
      entity.HasKey(f => new { f.FollowerId, f.FollowingId });
      entity.HasOne(f => f.Follower)
        .WithMany(u => u.Following)
        .HasForeignKey(f => f.FollowerId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(f => f.Following)
        .WithMany(u => u.Followers)
        .HasForeignKey(f => f.FollowingId)
        .OnDelete(DeleteBehavior.Restrict);

      entity.ToTable(t => t.HasCheckConstraint("CK_Follow_NoSelfFollow",
        "[FollowerId] != [FollowingId]"));
    });

    builder.Entity<RefreshToken>(entity =>
    {
      entity.HasKey(r => r.Id);
      entity.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
      entity.Property(rt => rt.UserId).IsRequired();
      entity.Property(rt => rt.CreatedByIp).HasMaxLength(45);
      entity.Property(rt => rt.RevokedByIp).HasMaxLength(45);

      entity.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

      entity.HasIndex(rt => rt.Token).IsUnique();
      entity.HasIndex(rt => rt.UserId);
      entity.HasIndex(rt => rt.ExpiresAt);
    });
  }
}