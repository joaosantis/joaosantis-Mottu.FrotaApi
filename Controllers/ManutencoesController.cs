using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManutencoesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ManutencoesController> _logger;

    public ManutencoesController(AppDbContext db, ILogger<ManutencoesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // DTOs simples (no mesmo arquivo)
    public class ManutencaoCreateDto
    {
        [Required, MaxLength(500)]
        public string Descricao { get; set; } = default!;

        [Required]
        public DateTime Data { get; set; }

        [Required]
        public int MotoId { get; set; }
    }
    public class ManutencaoUpdateDto : ManutencaoCreateDto { }

    public class ManutencaoListDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = default!;
        public DateTime Data { get; set; }
        public int MotoId { get; set; }
        public string? MotoPlaca { get; set; }
        public string? MotoModelo { get; set; }
    }

    // GET: api/Manutencoes?motoId=1&from=2025-09-01&to=2025-09-30&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<object>> Get(
        [FromQuery] int? motoId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        try
        {
            var q = _db.Manutencoes.AsNoTracking().Include(m => m.Moto).AsQueryable();

            if (motoId.HasValue) q = q.Where(m => m.MotoId == motoId.Value);
            if (from.HasValue)   q = q.Where(m => m.Data >= from.Value);
            if (to.HasValue)     q = q.Where(m => m.Data <= to.Value);

            var total = await q.CountAsync(ct);

            var itens = await q
                .OrderByDescending(m => m.Data)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ManutencaoListDto
                {
                    Id = m.Id,
                    Descricao = m.Descricao,
                    Data = m.Data,
                    MotoId = m.MotoId,
                    MotoPlaca = m.Moto!.Placa,
                    MotoModelo = m.Moto!.Modelo
                })
                .ToListAsync(ct);

            return Ok(new { total, page, pageSize, itens });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao listar manutenÃ§Ãµes. TraceId={TraceId}", HttpContext.TraceIdentifier);
            return Problem(title: "Falha ao listar manutenÃ§Ãµes.",
                           detail: ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError,
                           extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
        }
    }

    // GET: api/Manutencoes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ManutencaoListDto>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var item = await _db.Manutencoes
                .AsNoTracking()
                .Include(m => m.Moto)
                .Where(m => m.Id == id)
                .Select(m => new ManutencaoListDto
                {
                    Id = m.Id,
                    Descricao = m.Descricao,
                    Data = m.Data,
                    MotoId = m.MotoId,
                    MotoPlaca = m.Moto!.Placa,
                    MotoModelo = m.Moto!.Modelo
                })
                .FirstOrDefaultAsync(ct);

            return item is null ? NotFound() : Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao buscar manutenÃ§Ã£o {Id}. TraceId={TraceId}", id, HttpContext.TraceIdentifier);
            return Problem(title: $"Falha ao buscar manutenÃ§Ã£o {id}.",
                           detail: ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError,
                           extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
        }
    }

    // POST: api/Manutencoes
    [HttpPost]
    public async Task<ActionResult<ManutencaoListDto>> Post([FromBody] ManutencaoCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var moto = await _db.Motos.FindAsync(new object?[] { dto.MotoId }, ct);
            if (moto is null)
            {
                ModelState.AddModelError(nameof(dto.MotoId), "Moto nÃ£o encontrada.");
                return ValidationProblem(ModelState);
            }

            var entity = new Manutencao
            {
                Descricao = dto.Descricao,
                Data = dto.Data,
                MotoId = dto.MotoId
            };

            _db.Manutencoes.Add(entity);
            await _db.SaveChangesAsync(ct);

            var result = new ManutencaoListDto
            {
                Id = entity.Id,
                Descricao = entity.Descricao,
                Data = entity.Data,
                MotoId = entity.MotoId,
                MotoPlaca = moto.Placa,
                MotoModelo = moto.Modelo
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar manutenÃ§Ã£o. Payload={@dto}", dto);
            return Problem(title: "Erro ao salvar manutenÃ§Ã£o.",
                           detail: ex.InnerException?.Message ?? ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao criar manutenÃ§Ã£o. TraceId={TraceId}", HttpContext.TraceIdentifier);
            return Problem(title: "Falha ao criar manutenÃ§Ã£o.",
                           detail: ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError,
                           extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
        }
    }

    // PUT: api/Manutencoes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] ManutencaoUpdateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var entity = await _db.Manutencoes.FindAsync(new object?[] { id }, ct);
            if (entity is null) return NotFound();

            var moto = await _db.Motos.FindAsync(new object?[] { dto.MotoId }, ct);
            if (moto is null)
            {
                ModelState.AddModelError(nameof(dto.MotoId), "Moto nÃ£o encontrada.");
                return ValidationProblem(ModelState);
            }

            entity.Descricao = dto.Descricao;
            entity.Data = dto.Data;
            entity.MotoId = dto.MotoId;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Conflito de concorrÃªncia ao atualizar manutenÃ§Ã£o {Id}.", id);
            return Problem(title: "Conflito de concorrÃªncia.",
                           detail: ex.Message,
                           statusCode: StatusCodes.Status409Conflict);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao atualizar manutenÃ§Ã£o {Id}.", id);
            return Problem(title: "Erro ao salvar alteraÃ§Ãµes.",
                           detail: ex.InnerException?.Message ?? ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao atualizar manutenÃ§Ã£o {Id}. TraceId={TraceId}", id, HttpContext.TraceIdentifier);
            return Problem(title: $"Falha ao atualizar manutenÃ§Ã£o {id}.",
                           detail: ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError,
                           extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
        }
    }

    // DELETE: api/Manutencoes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _db.Manutencoes.FindAsync(new object?[] { id }, ct);
            if (entity is null) return NotFound();

            _db.Manutencoes.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao excluir manutenÃ§Ã£o {Id}.", id);
            return Problem(title: "Erro ao excluir manutenÃ§Ã£o.",
                           detail: ex.InnerException?.Message ?? ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao excluir manutenÃ§Ã£o {Id}. TraceId={TraceId}", id, HttpContext.TraceIdentifier);
            return Problem(title: $"Falha ao excluir manutenÃ§Ã£o {id}.",
                           detail: ex.Message,
                           statusCode: StatusCodes.Status500InternalServerError,
                           extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
        }
    }
}
