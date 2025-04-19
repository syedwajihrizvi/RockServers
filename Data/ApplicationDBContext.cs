using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using RockServers.Models;

namespace RockServers.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        // Adds to the Models
        public DbSet<Game> Games { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DiscussionComment> DiscussionComments { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionUser> SessionUsers { get; set; }

        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Images> Images { get; set; }
        // Override model builder to account for the user
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            List<IdentityRole> roles =
            [
                new IdentityRole{Name = "Admin", NormalizedName = "ADMIN"},
                new IdentityRole{Name = "User", NormalizedName = "USER"}
            ];

            modelBuilder.Entity<IdentityRole>().HasData(roles);
            modelBuilder.Entity<SessionUser>()
                        .HasKey(s => new { s.SessionId, s.AppUserId });
            modelBuilder.Entity<Session>()
                        .HasOne(s => s.Post)
                        .WithMany(p => p.Sessions)
                        .HasForeignKey(s => s.PostId)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                        .HasOne(p => p.AppUser)
                        .WithMany()
                        .HasForeignKey(p => p.AppUserId)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                        .HasMany(p => p.LikedByUsers)
                        .WithMany(a => a.LikedPosts)
                        .UsingEntity(j => j.ToTable("PostLiked"));
            modelBuilder.Entity<Discussion>()
                        .HasOne(p => p.AppUser)
                        .WithMany()
                        .HasForeignKey(p => p.AppUserId)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Discussion>()
                        .HasMany(d => d.LikedByUsers)
                        .WithMany(a => a.LikedDicussions)
                        .UsingEntity(j => j.ToTable("DiscussionLiked"));
            modelBuilder.Entity<Discussion>()
                        .HasOne(d => d.AppUser)
                        .WithMany()
                        .HasForeignKey(d => d.AppUserId)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>()
                        .HasMany(c => c.LikedByUsers)
                        .WithMany(a => a.LikedComments)
                        .UsingEntity(j => j.ToTable("CommentLiked"));
            modelBuilder.Entity<Comment>()
                        .HasOne(c => c.AppUser)
                        .WithMany()
                        .HasForeignKey(c => c.AppUserId)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiscussionComment>()
                        .HasMany(c => c.LikedByUsers)
                        .WithMany(a => a.LikedDiscussionComments)
                        .UsingEntity(j => j.ToTable("DiscussionCommentLiked"));
            modelBuilder.Entity<DiscussionComment>()
                        .HasOne(c => c.AppUser)
                        .WithMany()
                        .HasForeignKey(c => c.AppUserId)
                        .OnDelete(DeleteBehavior.Cascade);

        }
    }
}