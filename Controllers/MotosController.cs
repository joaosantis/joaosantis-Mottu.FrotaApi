using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotosController : ApiControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<MotosController> _logger;

        public MotosController(AppDbContext db, ILogger<MotosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/Motos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Moto>>> Get(CancellationToken ct)
        {
            try
            {
                var list = await _db.Motos
                    .AsNoTracking()
                    .Include(m => m.Filial)
                    .ToListAsync(ct);

                return Ok(list);
            }
            catch (Exception ex)
            {
                return Problem500(ex, "Falha ao listar motos.");
            }
        }

        // GET: api/Motos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Moto>> Get(int id, CancellationToken ct)
        {
            try
            {
                var moto = await _db.Motos
                    .AsNoTracking()
                    .Include(m => m.Filial)
                    .FirstOrDefaultAsync(m => m.Id == id, ct);

                return moto is null ? NotFound() : Ok(moto);
            }
            catch (Exception ex)
            {
                return Problem500(ex, $"Falha ao buscar a moto {id}.");
            }
        }

        // POST: api/Motos
        [HttpPost]
        public async Task<ActionResult<Moto>> Post([FromBody] Moto moto, CancellationToken ct)
        {
            try
            {
                // Valida FK antes de salvar
                var filialExiste = await _db.Filiais.AnyAsync(f => f.Id == moto.FilialId, ct);
                if (!filialExiste)
                    return BadRequest(new { error = "FilialId invÃ¡lido. Cadastre uma filial existente e informe seu Id." });

                _db.Motos.Add(moto);
                await _db.SaveChangesAsync(ct);

                // carrega navegaÃ§Ã£o para retornar completa
                await _db.Entry(moto).Reference(m => m.Filial).LoadAsync(ct);

                return CreatedAtAction(nameof(Get), new { id = moto.Id }, moto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de persistÃªncia ao criar moto. Payload={@moto}", moto);
                return Problem(
                    detail: ex.InnerException?.Message ?? ex.Message,
                    title: "Erro ao salvar moto.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return Problem500(ex, "Falha ao criar moto.");
            }
        }

        // PUT: api/Motos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Moto moto, CancellationToken ct)
        {
            if (id != moto.Id)
                return BadRequest(new { error = "O id da rota difere do id do corpo." });

            try
            {
                var existe = await _db.Motos.AnyAsync(m => m.Id == id, ct);
                if (!existe) return NotFound();

                // (opcional) validar FK tambÃ©m no update
                var filialExiste = await _db.Filiais.AnyAsync(f => f.Id == moto.FilialId, ct);
                if (!filialExiste)
                    return BadRequest(new { error = "FilialId invÃ¡lido." });

                _db.Entry(moto).State = EntityState.Modified;

                await _db.SaveChangesAsync(ct);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "ConcorrÃªncia ao atualizar moto {Id}.", id);
                return Problem(
                    title: "Conflito de concorrÃªncia ao atualizar.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de persistÃªncia ao atualizar moto {Id}.", id);
                return Problem(
                    detail: ex.InnerException?.Message ?? ex.Message,
                    title: "Erro ao salvar alteraÃ§Ãµes.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return Problem500(ex, $"Falha ao atualizar a moto {id}.");
            }
        }

        // DELETE: api/Motos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var moto = await _db.Motos.FindAsync(new object?[] { id }, ct);
                if (moto is null) return NotFound();

                _db.Motos.Remove(moto);
                await _db.SaveChangesAsync(ct);

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de persistÃªncia ao excluir moto {Id}.", id);
                return Problem(
                    detail: ex.InnerException?.Message ?? ex.Message,
                    title: "Erro ao excluir moto.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                return Problem500(ex, $"Falha ao excluir a moto {id}.");
            }
        }

        // -------------------------
        // Helpers
        // -------------------------
        private ObjectResult Problem500(Exception ex, string title)
        {
            _logger.LogError(ex, "{Title} TraceId={TraceId}", title, HttpContext.TraceIdentifier);

            return Problem(
                title: title,
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = HttpContext.TraceIdentifier
                });
        }
    }
}
