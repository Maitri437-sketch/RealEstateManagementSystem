using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<PropertyComparison> PropertyComparisons { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Property relationships
            builder.Entity<Property>(entity =>
            {
                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.HasOne(p => p.Seller)
                    .WithMany(u => u.PropertiesListed)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Agent)
                    .WithMany()
                    .HasForeignKey(p => p.AgentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PropertyImage relationship
            builder.Entity<PropertyImage>(entity =>
            {
                entity.HasOne(pi => pi.Property)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Appointment relationships
            builder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Property)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Client)
                    .WithMany(u => u.ClientAppointments)
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Agent)
                    .WithMany(u => u.AgentAppointments)
                    .HasForeignKey(a => a.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Inquiry relationships
            builder.Entity<Inquiry>(entity =>
            {
                entity.HasOne(i => i.Property)
                    .WithMany(p => p.Inquiries)
                    .HasForeignKey(i => i.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Client)
                    .WithMany(u => u.Inquiries)
                    .HasForeignKey(i => i.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Wishlist relationships & unique constraint
            builder.Entity<Wishlist>(entity =>
            {
                entity.HasIndex(w => new { w.ClientId, w.PropertyId })
                    .IsUnique();

                entity.HasOne(w => w.Property)
                    .WithMany(p => p.Wishlists)
                    .HasForeignKey(w => w.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Client)
                    .WithMany(u => u.Wishlists)
                    .HasForeignKey(w => w.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PropertyComparison relationships & unique constraint
            builder.Entity<PropertyComparison>(entity =>
            {
                entity.HasIndex(pc => new { pc.ClientId, pc.PropertyId })
                    .IsUnique();

                entity.HasOne(pc => pc.Property)
                    .WithMany(p => p.Comparisons)
                    .HasForeignKey(pc => pc.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Client)
                    .WithMany(u => u.Comparisons)
                    .HasForeignKey(pc => pc.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Review relationships
            builder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Property)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Client)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification relationships
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}