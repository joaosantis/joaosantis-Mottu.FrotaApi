using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto> Motos => Set<Moto>();
        public DbSet<Filial> Filiais => Set<Filial>();
        public DbSet<Manutencao> Manutencoes => Set<Manutencao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
	modelBuilder.Entity<Moto>()
    .HasIndex(p => p.Placa)
    .IsUnique();


            // ===========
            // AJUSTES DE TAMANHO (iguais aos seus)
            // ===========
            modelBuilder.Entity<Moto>()
                .Property(p => p.Placa).HasMaxLength(20);

            modelBuilder.Entity<Moto>()
                .Property(p => p.Modelo).HasMaxLength(100);

            modelBuilder.Entity<Moto>()
                .Property(p => p.Status).HasMaxLength(50);

            modelBuilder.Entity<Filial>()
                .Property(p => p.Nome).HasMaxLength(200);

            modelBuilder.Entity<Filial>()
                .Property(p => p.Endereco).HasMaxLength(300);

            modelBuilder.Entity<Manutencao>()
                .Property(p => p.Descricao).HasMaxLength(500);

            // ===========
            // RELACIONAMENTOS
            // ===========

            // Moto (N) -> Filial (1)
            // Uma Filial tem muitas Motos; uma Moto pertence a uma Filial
            // Restrict: impede excluir a Filial se existir Moto vinculada
            modelBuilder.Entity<Moto>()
                .HasOne(m => m.Filial)
                .WithMany(f => f.Motos)
                .HasForeignKey(m => m.FilialId)
                .OnDelete(DeleteBehavior.Restrict);

            // Manutencao (N) -> Moto (1)
            // Cascade: ao excluir a Moto, as manutenções dela são excluídas
            modelBuilder.Entity<Manutencao>()
                .HasOne(x => x.Moto)
                .WithMany() // se quiser, crie ICollection<Manutencao> em Moto e troque por .WithMany(m => m.Manutencoes)
                .HasForeignKey(x => x.MotoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
