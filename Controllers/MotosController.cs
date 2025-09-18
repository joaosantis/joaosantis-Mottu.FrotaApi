using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MotosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Motos?page=1&pageSize=10
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetMotos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var totalItems = await _context.Motos.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var motos = await _context.Motos
                .Include(m => m.Filial)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                totalItems,
                page,
                pageSize,
                totalPages,
                data = motos.Select(m => new
                {
                    m.Id,
                    m.Placa,
                    m.Modelo,
                    m.Status,
                    Filial = new { m.Filial.Id, m.Filial.Nome },
                    links = new[]
                    {
                        new { rel = "self", href = Url.Action(nameof(GetMoto), new { id = m.Id }) },
                        new { rel = "update", href = Url.Action(nameof(UpdateMoto), new { id = m.Id }) },
                        new { rel = "delete", href = Url.Action(nameof(DeleteMoto), new { id = m.Id }) }
                    }
                }),
                links = new[]
                {
                    new { rel = "self", href = Url.Action(nameof(GetMotos), new { page, pageSize }) },
                    new { rel = "next", href = page < totalPages ? Url.Action(nameof(GetMotos), new { page = page + 1, pageSize }) : null },
                    new { rel = "prev", href = page > 1 ? Url.Action(nameof(GetMotos), new { page = page - 1, pageSize }) : null }
                }
            };

            return Ok(result);
        }

        // GET: api/Motos/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMoto(int id)
        {
            var moto = await _context.Motos.Include(m => m.Filial).FirstOrDefaultAsync(m => m.Id == id);
            if (moto == null) return NotFound();

            var result = new
            {
                moto.Id,
                moto.Placa,
                moto.Modelo,
                moto.Status,
                Filial = new { moto.Filial.Id, moto.Filial.Nome },
                links = new[]
                {
                    new { rel = "self", href = Url.Action(nameof(GetMoto), new { id = moto.Id }) },
                    new { rel = "update", href = Url.Action(nameof(UpdateMoto), new { id = moto.Id }) },
                    new { rel = "delete", href = Url.Action(nameof(DeleteMoto), new { id = moto.Id }) }
                }
            };

            return Ok(result);
        }

        // POST: api/Motos
        [HttpPost]
        [ProducesResponseType(typeof(Moto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateMoto([FromBody] Moto moto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, moto);
        }

        // PUT: api/Motos/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMoto(int id, [FromBody] Moto moto)
        {
            if (id != moto.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(moto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Motos.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Motos/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}