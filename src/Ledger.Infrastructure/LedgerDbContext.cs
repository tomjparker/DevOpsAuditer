using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure;

public class LedgerDbContext: DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options) { }

    public DbSet<Service> Services => Set<Service>();
    public DbSet<EnvironmentEntity> Environments => Set<EnvironmentEntity>();
    public DbSet<Release> Releases => Set<Release>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>().HasIndex(s => s.Name).IsUnique();
        modelBuilder.Entity<EnvironmentEntity>().HasIndex(e => e.Name).IsUnique();

        modelBuilder.Entity<Release>()
            .HasOne(r => r.Service).WithMany(s => s.Releases)
            .HasForeignKey(r => r.ServiceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Release>()
            .HasOne(r => r.Environment).WithMany(e => e.Releases)
            .HasForeignKey(r => r.EnvironmentId).OnDelete(DeleteBehavior.Restrict);
    }
}