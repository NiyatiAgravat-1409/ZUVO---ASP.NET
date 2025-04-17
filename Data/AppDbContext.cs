using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZUVO.Models;
using ZUVO_MVC_.Models;

namespace ZUVO_MVC_.Data
{
    public class AppDbContext : IdentityDbContext<Users, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<HostUser> HostUsers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Query> Queries { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarPhoto> CarPhotos { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<CarType> CarTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Users entity
            builder.Entity<Users>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ProfilePicPath).HasMaxLength(200);
            });

            // Configure HostUser entity
            builder.Entity<HostUser>(entity =>
            {
                entity.ToTable("HostUsers");
                entity.HasKey(e => e.HostId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.WalletBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Transaction entity
            builder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("GETUTCDATE()");
                
                // Configure relationship
                entity.HasOne(e => e.HostUser)
                    .WithMany(e => e.Transactions)
                    .HasForeignKey(e => e.HostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure relationships
            builder.Entity<Car>()
                .HasOne(c => c.Host)
                .WithMany()
                .HasForeignKey(c => c.HostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPhoto>()
                .HasOne(p => p.Car)
                .WithMany(c => c.Photos)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

