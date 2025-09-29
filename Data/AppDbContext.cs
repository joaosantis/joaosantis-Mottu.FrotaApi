using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto> Motos => Set<Moto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Moto>().ToTable("Moto");
            base.OnModelCreating(modelBuilder);
        }
    }
}
