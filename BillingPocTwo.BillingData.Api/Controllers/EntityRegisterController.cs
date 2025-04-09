using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.BillingData.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntityRegisterController : Controller
    {
        private readonly IBillingDbContext _context;

        public EntityRegisterController(IBillingDbContext context)
        {
            _context = context;
        }

        // Route: GET api/EntityRegister
        [HttpGet]
        public async Task<IActionResult> GetAllEntityRegisters()
        {
            var entityRegisters = await _context.EntityRegisters
                .Where(e => e.ENTITY_TYPE == "ACCOUNT")
                .ToListAsync();

            return Ok(entityRegisters);
        }

        // Route: GET api/EntityRegister/paged
        [HttpGet("paged")]
        public async Task<IActionResult> GetEntityRegistersPaged(int page = 1, int pageSize = 10)
        {
            var query = _context.EntityRegisters
                .Where(e => e.ENTITY_TYPE == "ACCOUNT");

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                Items = items,
                TotalPages = totalPages
            };

            return Ok(response);
        }
    }
}
