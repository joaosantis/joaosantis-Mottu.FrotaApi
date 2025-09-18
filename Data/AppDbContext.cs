using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<Filial> Filiais { get; set; }
        public DbSet<Manutencao> Manutencoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FILIAL
            modelBuilder.Entity<Filial>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).HasColumnType("NUMBER").ValueGeneratedOnAdd();
                entity.Property(f => f.Nome).HasColumnType("NVARCHAR2(200)").IsRequired();
                entity.Property(f => f.Endereco).HasColumnType("NVARCHAR2(300)").IsRequired();
                entity.HasMany(f => f.Motos)
                      .WithOne(m => m.Filial)
                      .HasForeignKey(m => m.FilialId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MOTO
            modelBuilder.Entity<Moto>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).HasColumnType("NUMBER").ValueGeneratedOnAdd();
                entity.Property(m => m.Placa).HasColumnType("NVARCHAR2(20)").IsRequired();
                entity.Property(m => m.Modelo).HasColumnType("NVARCHAR2(100)").IsRequired();
                entity.Property(m => m.Status).HasColumnType("NVARCHAR2(50)").IsRequired();
                entity.HasMany(m => m.Manutencoes)
                      .WithOne(ma => ma.Moto)
                      .HasForeignKey(ma => ma.MotoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MANUTENCAO
            modelBuilder.Entity<Manutencao>(entity =>
            {
                entity.HasKey(ma => ma.Id);
                entity.Property(ma => ma.Id).HasColumnType("NUMBER").ValueGeneratedOnAdd();
                entity.Property(ma => ma.Descricao).HasColumnType("NVARCHAR2(500)").IsRequired();
                entity.Property(ma => ma.Data).HasColumnType("DATE").IsRequired();
            });
        }
    }
}
