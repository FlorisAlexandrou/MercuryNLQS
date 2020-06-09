using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Speech2TextPrototype.Data;

namespace Speech2TextPrototype.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TDataController : ControllerBase
    {
        private readonly florisContext _context;

        public TDataController(florisContext context)
        {
            _context = context;
        }

        // GET: api/TData
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TData>>> Gettdata()
        {
            return await _context.tdata.ToListAsync();
        }

        // GET: api/TData/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TData>> GetTData(int id)
        {
            string brand = "Agros";
            var q = from td in _context.tdata
                     where td.BRAND == brand
                     select td;

            var tData = await _context.tdata.FindAsync(id);

            if (tData == null)
            {
                return NotFound();
            }

            return tData;
        }

        // PUT: api/TData/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTData(int id, TData tData)
        {
            if (id != tData.TID)
            {
                return BadRequest();
            }

            _context.Entry(tData).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TDataExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TData
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<TData>> PostTData(TData tData)
        {
            _context.tdata.Add(tData);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTData", new { id = tData.TID }, tData);
        }

        // DELETE: api/TData/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TData>> DeleteTData(int id)
        {
            var tData = await _context.tdata.FindAsync(id);
            if (tData == null)
            {
                return NotFound();
            }

            _context.tdata.Remove(tData);
            await _context.SaveChangesAsync();

            return tData;
        }

        private bool TDataExists(int id)
        {
            return _context.tdata.Any(e => e.TID == id);
        }
    }
}
