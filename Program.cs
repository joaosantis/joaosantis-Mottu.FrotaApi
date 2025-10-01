// Program.cs
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mottu.FrotaApi.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Faz o Kestrel escutar na porta que o Azure define (ou 8080 local/Docker)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Controllers + System.Text.Json
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mottu.FrotaApi", Version = "v1" });
});

// DbContext usando a ConnectionStrings:DefaultConnection (Azure SQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Encaminhar cabeçalhos do proxy (App Service Linux)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger sempre habilitado (útil para testes/demo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu.FrotaApi v1");
    c.RoutePrefix = "swagger";
});

// HTTPS redirection (ok no Azure por causa do ForwardedHeaders)
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// Redireciona raiz para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));


// === APLICA AS MIGRAÇÕES NA SUBIDA DA APLICAÇÃO ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // cria/atualiza o schema no Azure SQL
}

app.Run();
