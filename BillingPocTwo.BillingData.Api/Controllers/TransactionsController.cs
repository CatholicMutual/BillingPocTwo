using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.DataObjects.Billing;
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

                //// Step 2: Use POLICY_ENTITY_REGISTER to find POLICY_TERM_IDs associated with the SYSTEM_ENTITY_CODE
                //var policyTermIds = await _context.PolicyEntityIntermediate
                //    .Where(pe => pe.SYSTEM_ENTITY_CODE == systemEntityCode)
                //    .Select(pe => pe.POLICY_TERM_ID)
                //    .Distinct()
                //    .ToListAsync();

                //if (!policyTermIds.Any())
                //{
                //    return NotFound($"No POLICY_TERM_IDs found for SYSTEM_ENTITY_CODE: {systemEntityCode}");
                //}

                // Step 3: Use POLICY_TERM_IDs to query TRANSACTION_LOG table
                var transactionsQuery = _context.TransactionLogs
                    .Where(tl => tl.ACCOUNT_SYSTEM_CODE == systemEntityCode);

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

        [HttpGet("combined/{sourceSystemEntityCode}")]
        public async Task<IActionResult> GetCombinedTransactions(string sourceSystemEntityCode, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(sourceSystemEntityCode))
                return BadRequest("SOURCE_SYSTEM_ENTITY_CODE cannot be null or empty.");

            // Find SYSTEM_ENTITY_CODE
            var systemEntityCode = await _context.EntityRegisters
                .Where(e => e.SOURCE_SYSTEM_ENTITY_CODE == sourceSystemEntityCode)
                .Select(e => e.SYSTEM_ENTITY_CODE)
                .FirstOrDefaultAsync();

            if (systemEntityCode == 0)
                return NotFound($"No SYSTEM_ENTITY_CODE found for SOURCE_SYSTEM_ENTITY_CODE: {sourceSystemEntityCode}");

            // Find POLICY_TERM_IDs
            var policyTermIds = await _context.PolicyEntityIntermediate
                .Where(pe => pe.SYSTEM_ENTITY_CODE == systemEntityCode)
                .Select(pe => pe.POLICY_TERM_ID)
                .Distinct()
                .ToListAsync();

            // Get Policy Numbers
            var policyNumbers = await _context.PolicyRegisters
                .Where(pr => policyTermIds.Contains(pr.POLICY_TERM_ID))
                .Select(pr => pr.POLICY_NO)
                .Distinct()
                .ToListAsync();

            // TRANSACTION_LOGs
            var transactionLogsQuery = _context.TransactionLogs
                .Where(tl => policyNumbers.Contains(tl.POLICY_NO));
            if (fromDate.HasValue)
                transactionLogsQuery = transactionLogsQuery.Where(tl => tl.CREATED_ON >= fromDate.Value);
            if (toDate.HasValue)
                transactionLogsQuery = transactionLogsQuery.Where(tl => tl.CREATED_ON <= toDate.Value);
            var transactionLogs = await transactionLogsQuery.OrderBy(tl => tl.CREATED_ON).ToListAsync();

            // Get SYSTEM_TRANSACTION_SEQs from the filtered transaction logs
            var transactionSeqs = transactionLogs.Select(tl => tl.SYSTEM_TRANSACTION_SEQ).ToList();

            // ASSIGNED_PAYMENTs that match SYSTEM_TRANSACTION_SEQ and account
            var assignedPayments = await _context.AssignedPayments
                .Where(ap => transactionSeqs.Contains(ap.SYSTEM_TRANSACTION_SEQ))
                .ToListAsync();

            var result = new AccountTransactionsDto
            {
                TransactionLogs = transactionLogs,
                AssignedPayments = assignedPayments
            };

            return Ok(result);
        }

        [HttpGet("byaccountnumber/account/{accountSystemCode}")]
        public async Task<IActionResult> GetTransactionsByAccountSystemCode(decimal accountSystemCode, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            if (accountSystemCode == 0)
                return BadRequest("ACCOUNT_SYSTEM_CODE cannot be zero.");

            try
            {
                var transactionsQuery = _context.TransactionLogs
                    .Where(tl => tl.ACCOUNT_SYSTEM_CODE == accountSystemCode);

                if (fromDate.HasValue)
                    transactionsQuery = transactionsQuery.Where(tl => tl.CREATED_ON >= fromDate.Value);

                if (toDate.HasValue)
                    transactionsQuery = transactionsQuery.Where(tl => tl.CREATED_ON <= toDate.Value);

                var transactions = await transactionsQuery
                    .OrderBy(tl => tl.CREATED_ON)
                    .ToListAsync();

                if (!transactions.Any())
                    return NotFound("No transactions found for the given account number.");

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
