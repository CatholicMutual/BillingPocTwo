using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.BillingData.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly IBillingDbContext _context;

        public TransactionsController(IBillingDbContext context)
        {
            _context = context;
        }

        // Route: GET api/EntityAddress/addresses/{sourceSystemEntityCode}
        [HttpGet("byaccountnumber/{sourceSystemEntityCode}")]
        public async Task<IActionResult> GetTransactionsBySourceSystemEntityCode(
            string sourceSystemEntityCode,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(sourceSystemEntityCode))
            {
                return BadRequest("SOURCE_SYSTEM_ENTITY_CODE cannot be null or empty.");
            }

            try
            {
                // Step 1: Find the SYSTEM_ENTITY_CODE for the given SOURCE_SYSTEM_ENTITY_CODE
                var systemEntityCode = await _context.EntityRegisters
                    .Where(e => e.SOURCE_SYSTEM_ENTITY_CODE == sourceSystemEntityCode)
                    .Select(e => e.SYSTEM_ENTITY_CODE)
                    .FirstOrDefaultAsync();

                if (systemEntityCode == 0)
                {
                    return NotFound($"No SYSTEM_ENTITY_CODE found for SOURCE_SYSTEM_ENTITY_CODE: {sourceSystemEntityCode}");
                }

                // Step 2: Use POLICY_ENTITY_REGISTER to find POLICY_TERM_IDs associated with the SYSTEM_ENTITY_CODE
                var policyTermIds = await _context.PolicyEntityIntermediate
                    .Where(pe => pe.SYSTEM_ENTITY_CODE == systemEntityCode)
                    .Select(pe => pe.POLICY_TERM_ID)
                    .Distinct()
                    .ToListAsync();

                if (!policyTermIds.Any())
                {
                    return NotFound($"No POLICY_TERM_IDs found for SYSTEM_ENTITY_CODE: {systemEntityCode}");
                }

                // Step 3: Use POLICY_TERM_IDs to query TRANSACTION_LOG table
                var transactionsQuery = _context.TransactionLogs
                    .Where(tl => tl.POLICY_TERM_ID.HasValue && policyTermIds.Contains(tl.POLICY_TERM_ID.Value));

                // Apply date filters if provided
                if (fromDate.HasValue)
                {
                    transactionsQuery = transactionsQuery.Where(tl => tl.CREATED_ON >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    transactionsQuery = transactionsQuery.Where(tl => tl.CREATED_ON <= toDate.Value);
                }

                // Sort by CREATED_ON in ascending order
                var transactions = await transactionsQuery
                    .OrderBy(tl => tl.CREATED_ON)
                    .ToListAsync();

                if (!transactions.Any())
                {
                    return NotFound($"No transactions found for the given criteria.");
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
