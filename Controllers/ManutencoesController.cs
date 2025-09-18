using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManutencoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManutencoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Manutencoes?page=1&pageSize=10
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetManutencoes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var totalItems = await _context.Manutencoes.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var manutencoes = await _context.Manutencoes
                .Include(m => m.Moto)
                .ThenInclude(m => m.Filial)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                totalItems,
                page,
                pageSize,
                totalPages,
                data = manutencoes.Select(m => new
                {
                    m.Id,
                    m.Data,
                    m.Descricao,
                    Moto = new { m.Moto.Id, m.Moto.Placa, Filial = new { m.Moto.Filial.Id, m.Moto.Filial.Nome } },
                    links = new[]
                    {
                        new { rel = "self", href = Url.Action(nameof(GetManutencao), new { id = m.Id }) },
                        new { rel = "update", href = Url.Action(nameof(UpdateManutencao), new { id = m.Id }) },
                        new { rel = "delete", href = Url.Action(nameof(DeleteManutencao), new { id = m.Id }) }
                    }
                }),
                links = new[]
                {
                    new { rel = "self", href = Url.Action(nameof(GetManutencoes), new { page, pageSize }) },
                    new { rel = "next", href = page < totalPages ? Url.Action(nameof(GetManutencoes), new { page = page + 1, pageSize }) : null },
                    new { rel = "prev", href = page > 1 ? Url.Action(nameof(GetManutencoes), new { page = page - 1, pageSize }) : null }
                }
            };

            return Ok(result);
        }

        // GET: api/Manutencoes/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetManutencao(int id)
        {
            var manutencao = await _context.Manutencoes
                .Include(m => m.Moto)
                .ThenInclude(m => m.Filial)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manutencao == null) return NotFound();

            var result = new
            {
                manutencao.Id,
                manutencao.Data,
                manutencao.Descricao,
                Moto = new { manutencao.Moto.Id, manutencao.Moto.Placa, Filial = new { manutencao.Moto.Filial.Id, manutencao.Moto.Filial.Nome } },
                links = new[]
                {
                    new { rel = "self", href = Url.Action(nameof(GetManutencao), new { id = manutencao.Id }) },
                    new { rel = "update", href = Url.Action(nameof(UpdateManutencao), new { id = manutencao.Id }) },
                    new { rel = "delete", href = Url.Action(nameof(DeleteManutencao), new { id = manutencao.Id }) }
                }
            };

            return Ok(result);
        }

        // POST: api/Manutencoes
        [HttpPost]
        [ProducesResponseType(typeof(Manutencao), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateManutencao([FromBody] Manutencao manutencao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Manutencoes.Add(manutencao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetManutencao), new { id = manutencao.Id }, manutencao);
        }

        // PUT: api/Manutencoes/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateManutencao(int id, [FromBody] Manutencao manutencao)
        {
            if (id != manutencao.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(manutencao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Manutencoes.Any(m => m.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Manutencoes/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteManutencao(int id)
        {
            var manutencao = await _context.Manutencoes.FindAsync(id);
            if (manutencao == null) return NotFound();

            _context.Manutencoes.Remove(manutencao);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
