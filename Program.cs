using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Configurar DbContext com Oracle ou outro banco
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FrotaDb")); // Para testes, depois pode trocar para Oracle

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mottu Frota API",
        Version = "v1",
        Description = "API RESTful para gerenciamento de frota, motos, filiais e manutenções"
    });

    // Payload de Moto
    c.MapType<Moto>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(1) },
            ["placa"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("ABC-1234") },
            ["modelo"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Honda CG 160") },
            ["status"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Disponível") },
            ["filialId"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(1) }
        },
        Required = new HashSet<string> { "placa", "modelo", "status", "filialId" }
    });

    // Payload de Filial
    c.MapType<Filial>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(1) },
            ["nome"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Filial Central") },
            ["endereco"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Rua das Flores, 123") }
        },
        Required = new HashSet<string> { "nome", "endereco" }
    });

    // Payload de Manutencao
    c.MapType<Manutencao>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(1) },
            ["descricao"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Troca de óleo") },
            ["data"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString(DateTime.UtcNow.ToString("o")) },
            ["motoId"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(1) },
            ["filialId"] = new OpenApiSchema { Type = "integer", Example = new OpenApiInteger(1) }
        },
        Required = new HashSet<string> { "descricao", "data", "motoId" }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu Frota API V1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
