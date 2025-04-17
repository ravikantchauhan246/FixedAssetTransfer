using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FixedAssetTransfer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AssetTransfer> AssetTransfers { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure composite key for RolePermission
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });
                
            // Configure relationships
            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);
                
            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Configure AssetTransfer relationships
            builder.Entity<AssetTransfer>()
                .HasOne(at => at.Requestor)
                .WithMany()
                .HasForeignKey(at => at.RequestorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssetTransfer>()
                .HasOne(at => at.Accountant)
                .WithMany()
                .HasForeignKey(at => at.AccountantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssetTransfer>()
                .HasOne(at => at.Recipient)
                .WithMany()
                .HasForeignKey(at => at.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssetTransfer>()
                .HasOne(at => at.RejectedBy)
                .WithMany()
                .HasForeignKey(at => at.RejectedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}