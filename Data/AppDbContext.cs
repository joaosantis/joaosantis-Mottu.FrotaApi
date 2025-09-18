using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Filial> Filiais { get; set; }
    public DbSet<Moto> Motos { get; set; }
    public DbSet<Manutencao> Manutencoes { get; set; }
}