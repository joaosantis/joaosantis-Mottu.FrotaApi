using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mottu.FrotaApi.Data;
using Mottu.FrotaApi.Models;

namespace Mottu.FrotaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MotosController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _db.Motos.AsNoTracking().ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var moto = await _db.Motos.FindAsync(id);
            return moto is null ? NotFound() : Ok(moto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Moto moto)
        {
            _db.Motos.Add(moto);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = moto.Id }, moto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Moto moto)
        {
            if (id != moto.Id) return BadRequest();
            _db.Entry(moto).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var moto = await _db.Motos.FindAsync(id);
            if (moto is null) return NotFound();
            _db.Motos.Remove(moto);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
