using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodoController : ControllerBase
    {
        private readonly ApiDbContext _context;
        public TodoController(ApiDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.Items.ToListAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(ItemData data)
        {
            if (ModelState.IsValid)
            {
                await _context.Items.AddAsync(data);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetItem", new { data.Id }, data);
            }
            return new JsonResult("Something went wrong!")
            {
                StatusCode = 500
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                NotFound();
            }
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, ItemData data)
        {
            if (data.Id != id)
            {
                return BadRequest();
            }
            var existItem = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (existItem == null)
            {
                return NotFound();
            }
            existItem.Title = data.Title;
            existItem.Done = data.Done;
            existItem.Description = data.Description;
            _context.Items.Update(existItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var existItem = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (existItem == null)
            {
                return NotFound();
            }
            _context.Items.Remove(existItem);
            await _context.SaveChangesAsync();
            return Ok(existItem);
        }
    }
}