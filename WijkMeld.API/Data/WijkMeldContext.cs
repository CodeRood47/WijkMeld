
using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Entities;
namespace WijkMeld.API.Data
{
    public class WijkMeldContext : DbContext
    {
        public WijkMeldContext(DbContextOptions<WijkMeldContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<StatusUpdate> StatusUpdates { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<IncidentPhoto> IncidentPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Incident>()
                     .OwnsOne(i => i.Location);

            modelBuilder.Entity<StatusUpdate>()
                .HasOne(su => su.Incident)
                .WithMany((i => i.StatusUpdates)) 
                .HasForeignKey(su => su.IncidentId);

            modelBuilder.Entity<StatusUpdate>()
                .HasOne(su => su.ChangedBy)
                .WithMany();

            modelBuilder.Entity<Incident>()
                .HasOne(i => i.User)
                .WithMany(u => u.Incidents)
                .HasForeignKey(i => i.UserId);

            modelBuilder.Entity<User>()
                .HasMany( u => u.Incidents)
                .WithOne(i => i.User)
                .HasForeignKey(i =>i.UserId);

            modelBuilder.Entity<IncidentPhoto>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<IncidentPhoto>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

        }

    }
}
