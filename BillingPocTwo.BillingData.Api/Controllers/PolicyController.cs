using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BillingPocTwo.BillingData.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        private readonly BillingDbContext _context;

        public PolicyController(BillingDbContext context)
        {
            _context = context;
        }

        // GET: api/Policy/BySystemEntityCode/{systemEntityCode}
        [HttpGet("BySystemEntityCode/{systemEntityCode}")]
        public async Task<IActionResult> GetPoliciesBySystemEntityCode(decimal systemEntityCode)
        {
            // Step 1: Filter PolicyEntityIntermediate by SYSTEM_ENTITY_CODE
            var policyEntityMatches = await _context.PolicyEntityIntermediate
                .Where(pe => pe.SYSTEM_ENTITY_CODE == systemEntityCode)
                .ToListAsync();

            if (!policyEntityMatches.Any())
            {
                return NotFound($"No entries found in POLICY_ENTITY_REGISTER for SYSTEM_ENTITY_CODE: {systemEntityCode}");
            }

            // Step 2: Extract POLICY_TERM_IDs from the filtered results
            var policyTermIds = policyEntityMatches.Select(pe => pe.POLICY_TERM_ID).Distinct();

            // Step 3: Query PolicyRegisters for matching POLICY_TERM_IDs
            var policies = await _context.PolicyRegisters
                .Where(pr => policyTermIds.Contains(pr.POLICY_TERM_ID))
                .ToListAsync();

            if (!policies.Any())
            {
                return NotFound($"No policies found in POLICY_REGISTER for SYSTEM_ENTITY_CODE: {systemEntityCode}");
            }

            return Ok(policies);
        }
    }
}
