using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiliaisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FiliaisController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Filiais?page=1&pageSize=10
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetFiliais([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var totalItems = await _context.Filiais.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var filiais = await _context.Filiais
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                totalItems,
                page,
                pageSize,
                totalPages,
                data = filiais.Select(f => new
                {
                    f.Id,
                    f.Nome,
                    f.Endereco,
                    links = new[]
                    {
                        new { rel = "self", href = Url.Action(nameof(GetFilial), new { id = f.Id }) },
                        new { rel = "update", href = Url.Action(nameof(UpdateFilial), new { id = f.Id }) },
                        new { rel = "delete", href = Url.Action(nameof(DeleteFilial), new { id = f.Id }) }
                    }
                }),
                links = new[]
                {
                    new { rel = "self", href = Url.Action(nameof(GetFiliais), new { page, pageSize }) },
                    new { rel = "next", href = page < totalPages ? Url.Action(nameof(GetFiliais), new { page = page + 1, pageSize }) : null },
                    new { rel = "prev", href = page > 1 ? Url.Action(nameof(GetFiliais), new { page = page - 1, pageSize }) : null }
                }
            };

            return Ok(result);
        }

        // GET: api/Filiais/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFilial(int id)
        {
            var filial = await _context.Filiais.FindAsync(id);
            if (filial == null) return NotFound();

            var result = new
            {
                filial.Id,
                filial.Nome,
                filial.Endereco,
                links = new[]
                {
                    new { rel = "self", href = Url.Action(nameof(GetFilial), new { id = filial.Id }) },
                    new { rel = "update", href = Url.Action(nameof(UpdateFilial), new { id = filial.Id }) },
                    new { rel = "delete", href = Url.Action(nameof(DeleteFilial), new { id = filial.Id }) }
                }
            };

            return Ok(result);
        }

        // POST: api/Filiais
        [HttpPost]
        [ProducesResponseType(typeof(Filial), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateFilial([FromBody] Filial filial)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Filiais.Add(filial);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFilial), new { id = filial.Id }, filial);
        }

        // PUT: api/Filiais/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateFilial(int id, [FromBody] Filial filial)
        {
            if (id != filial.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(filial).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Filiais.Any(f => f.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Filiais/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFilial(int id)
        {
            var filial = await _context.Filiais.FindAsync(id);
            if (filial == null) return NotFound();

            _context.Filiais.Remove(filial);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
