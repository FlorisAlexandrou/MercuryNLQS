using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Speech2TextPrototype.Data;
using Speech2TextPrototype.Models;

namespace Speech2TextPrototype.Controllers
{
    [Route("api/lookup")]
    [ApiController]
    public class LookupValuesController : ControllerBase
    {
        private readonly florisContext _context;

        public LookupValuesController(florisContext context)
        {
            _context = context;
        }

        // GET: api/LookupValues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LookupValues>>> Getlookupvalues()
        {
            return await _context.lookupvalues.ToListAsync();
        }

        // GET: api/LookupValues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LookupValues>> GetLookupValues(string id)
        {
            var lookupValues = await _context.lookupvalues.FindAsync(id);

            if (lookupValues == null)
            {
                return NotFound();
            }

            return lookupValues;
        }

        [HttpGet]
        [Route("token/{text}")]
        public async Task<List<LookupValues>> tokenLookupAsync(string text)
        {
            string url = "https://tokens-api.herokuapp.com/tokenize/";

            using (HttpClient client = new HttpClient())
            {
                var httpResponse = await client.GetStringAsync(url + text);
                PyRes res = JsonConvert.DeserializeObject<PyRes>(httpResponse);
                string[] tokens = res.tokens;

                List<LookupValues> listKnownTokens = new List<LookupValues>();
                foreach (string token in tokens)
                {
                    LookupValues lookupValues = _context.lookupvalues.Where(row => row.Value == token).FirstOrDefault();
                    listKnownTokens.Add(lookupValues);
                }
                return listKnownTokens;
            }
        }

        // PUT: api/LookupValues/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLookupValues(string id, LookupValues lookupValues)
        {
            if (id != lookupValues.Value)
            {
                return BadRequest();
            }

            _context.Entry(lookupValues).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LookupValuesExists(id))
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

        // POST: api/LookupValues
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<LookupValues>> PostLookupValues(LookupValues lookupValues)
        {
            _context.lookupvalues.Add(lookupValues);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LookupValuesExists(lookupValues.Value))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetLookupValues", new { id = lookupValues.Value }, lookupValues);
        }

        // DELETE: api/LookupValues/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<LookupValues>> DeleteLookupValues(string id)
        {
            var lookupValues = await _context.lookupvalues.FindAsync(id);
            if (lookupValues == null)
            {
                return NotFound();
            }

            _context.lookupvalues.Remove(lookupValues);
            await _context.SaveChangesAsync();

            return lookupValues;
        }

        private bool LookupValuesExists(string id)
        {
            return _context.lookupvalues.Any(e => e.Value == id);
        }
    }
}
