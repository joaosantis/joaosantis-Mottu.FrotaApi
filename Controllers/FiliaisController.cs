using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiliaisController : ApiControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<FiliaisController> _logger;

        public FiliaisController(AppDbContext db, ILogger<FiliaisController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/Filiais?search=centro&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<object>> Get(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            try
            {
                var q = _db.Filiais.AsNoTracking().Include(f => f.Motos).AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    q = q.Where(f => f.Nome.Contains(search) || f.Endereco.Contains(search));

                var total = await q.CountAsync(ct);

                var itens = await q
                    .OrderBy(f => f.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                return Ok(new { total, page, pageSize, itens });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao listar filiais. TraceId={TraceId}", HttpContext.TraceIdentifier);
                return Problem(
                    title: "Falha ao listar filiais.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
            }
        }

        // GET: api/Filiais/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Filial>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var filial = await _db.Filiais
                                      .AsNoTracking()
                                      .Include(f => f.Motos)
                                      .FirstOrDefaultAsync(f => f.Id == id, ct);

                return filial is null ? NotFound() : Ok(filial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao buscar filial {Id}. TraceId={TraceId}", id, HttpContext.TraceIdentifier);
                return Problem(
                    title: $"Falha ao buscar filial {id}.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
            }
        }

        // POST: api/Filiais
        [HttpPost]
        public async Task<ActionResult<Filial>> Post([FromBody] Filial filial, CancellationToken ct)
        {
            try
            {
                _db.Filiais.Add(filial);
                await _db.SaveChangesAsync(ct);
                return CreatedAtAction(nameof(GetById), new { id = filial.Id }, filial);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao salvar filial. Payload={@filial}", filial);
                return Problem(title: "Erro ao salvar filial.",
                               detail: ex.InnerException?.Message ?? ex.Message,
                               statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar filial. TraceId={TraceId}", HttpContext.TraceIdentifier);
                return Problem(title: "Falha ao criar filial.",
                               detail: ex.Message,
                               statusCode: StatusCodes.Status500InternalServerError,
                               extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
            }
        }

        // PUT: api/Filiais/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Filial filial, CancellationToken ct)
        {
            if (id != filial.Id)
                return BadRequest(new { error = "O id da rota difere do id do corpo." });

            try
            {
                var exists = await _db.Filiais.AnyAsync(f => f.Id == id, ct);
                if (!exists) return NotFound();

                _db.Entry(filial).State = EntityState.Modified;
                await _db.SaveChangesAsync(ct);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflito de concorrÃªncia ao atualizar filial {Id}.", id);
                return Problem(title: "Conflito de concorrÃªncia.",
                               detail: ex.Message,
                               statusCode: StatusCodes.Status409Conflict);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar filial {Id}.", id);
                return Problem(title: "Erro ao salvar alteraÃ§Ãµes.",
                               detail: ex.InnerException?.Message ?? ex.Message,
                               statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao atualizar filial {Id}. TraceId={TraceId}", id, HttpContext.TraceIdentifier);
                return Problem(title: $"Falha ao atualizar filial {id}.",
                               detail: ex.Message,
                               statusCode: StatusCodes.Status500InternalServerError,
                               extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
            }
        }

        // DELETE: api/Filiais/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var filial = await _db.Filiais.Include(f => f.Motos).FirstOrDefaultAsync(f => f.Id == id, ct);
                if (filial is null) return NotFound();

                if (filial.Motos?.Any() == true)
                    return Conflict(new { error = "NÃ£o Ã© possÃ­vel excluir a filial: existem motos vinculadas." });

                _db.Filiais.Remove(filial);
                await _db.SaveChangesAsync(ct);

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao excluir filial {Id}.", id);
                return Problem(title: "Erro ao excluir filial.",
                               detail: ex.InnerException?.Message ?? ex.Message,
                               statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao excluir filial {Id}. TraceId={TraceId}", id, HttpContext.TraceIdentifier);
                return Problem(title: $"Falha ao excluir filial {id}.",
                               detail: ex.Message,
                               statusCode: StatusCodes.Status500InternalServerError,
                               extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier });
            }
        }
    }
}
