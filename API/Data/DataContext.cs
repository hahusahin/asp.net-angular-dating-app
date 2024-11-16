using System;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>,
    IdentityUserToken<int>>(options)
{
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Identity table configurations
        builder.Entity<AppUser>()
            .HasMany(user => user.UserRoles)
            .WithOne(userRole => userRole.User)
            .HasForeignKey(userRole => userRole.UserId)
            .IsRequired();

        builder.Entity<AppRole>()
            .HasMany(role => role.UserRoles)
            .WithOne(userRole => userRole.Role)
            .HasForeignKey(userRole => userRole.RoleId)
            .IsRequired();

        // Sets custom primary key
        builder.Entity<UserLike>()
            .HasKey(k => new { k.SourceUserId, k.TargetUserId });

        // Configure the two sides of many-to-many relationship
        // A single user can like many other users
        builder.Entity<UserLike>()
            .HasOne(s => s.SourceUser)
            .WithMany(l => l.LikedUsers)
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);
        // Many other users can like me as a single user
        builder.Entity<UserLike>()
            .HasOne(t => t.TargetUser)
            .WithMany(l => l.LikedByUsers)
            .HasForeignKey(t => t.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade); // sql server causes problem with cascade - use DeleteBehavior.NoAction

        // Configure the two sides of the both one-to-many relationships
        // A sinlge recipient can recieve multiple messages
        builder.Entity<Message>()
            .HasOne(x => x.Recipient)
            .WithMany(x => x.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
        // A single sender can send multiple messages
        builder.Entity<Message>()
            .HasOne(x => x.Sender)
            .WithMany(x => x.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
        // global query filter to get only approved photos
        builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
    }
}
