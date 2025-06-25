using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.AppUsers;
using Domain.Chat;
using Domain.Models.LinkFree;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
        { }

        public DbSet<Instagram> Instagram { get; set; }



        #region Users

        public DbSet<AppUser> AppUser { get; set; }

        #endregion

        #region Chat


        public DbSet<Messages> Messages { get; set; }

        public DbSet<Friends> Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Message and AppUser for Sender and Receiver
            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);  // No cascade delete for Sender

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);  // No cascade delete for Receiver


            modelBuilder.Entity<Friends>()
    .HasOne(f => f.appuserid)
    .WithMany()
    .HasForeignKey(f => f.UserId)
    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete

            modelBuilder.Entity<Friends>()
                .HasOne(f => f.appuserid2)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete
        }
        #endregion
    }
}
