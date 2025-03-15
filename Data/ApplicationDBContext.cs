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
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionUser> SessionUsers { get; set; }
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
        }
    }
}