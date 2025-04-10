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
        public async Task<IActionResult> GetEntityRegistersPaged(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? entityCode = null,
        decimal? minBalance = null,
        decimal? maxBalance = null,
        string? sortColumn = null,
        string? sortDirection = "asc")
        {
            var query = _context.EntityRegisters
                .Where(e => e.ENTITY_TYPE == "ACCOUNT");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.DOING_BUSINESS_AS_NAME.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(entityCode))
            {
                query = query.Where(e => e.SOURCE_SYSTEM_ENTITY_CODE.Contains(entityCode));
            }

            if (minBalance.HasValue)
            {
                query = query.Where(e => e.BALANCE >= minBalance.Value);
            }

            if (maxBalance.HasValue)
            {
                query = query.Where(e => e.BALANCE <= maxBalance.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortDirection == "asc"
                    ? query.OrderBy(e => EF.Property<object>(e, sortColumn))
                    : query.OrderByDescending(e => EF.Property<object>(e, sortColumn));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { Items = items, TotalPages = totalPages });
        }
    }
}
