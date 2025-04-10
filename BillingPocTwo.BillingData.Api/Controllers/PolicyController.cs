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

        [HttpGet("details/{sourceSystemEntityCode}/{policyTermId}")]
        public async Task<IActionResult> GetPolicyDetails(string sourceSystemEntityCode, decimal policyTermId)
        {
            // Step 1: Find the SYSTEM_ENTITY_CODE associated with the SOURCE_SYSTEM_ENTITY_CODE
            var entityRegister = await _context.EntityRegisters
                .FirstOrDefaultAsync(e => e.SOURCE_SYSTEM_ENTITY_CODE == sourceSystemEntityCode);

            if (entityRegister == null)
            {
                return NotFound($"No ENTITY_REGISTER found for SOURCE_SYSTEM_ENTITY_CODE: {sourceSystemEntityCode}");
            }

            // Step 2: Validate that the POLICY_TERM_ID belongs to the SYSTEM_ENTITY_CODE
            var isValidPolicy = await _context.PolicyEntityIntermediate
                .AnyAsync(pe => pe.POLICY_TERM_ID == policyTermId && pe.SYSTEM_ENTITY_CODE == entityRegister.SYSTEM_ENTITY_CODE);

            if (!isValidPolicy)
            {
                return BadRequest($"The POLICY_TERM_ID: {policyTermId} does not belong to the SYSTEM_ENTITY_CODE associated with the SOURCE_SYSTEM_ENTITY_CODE: {sourceSystemEntityCode}");
            }

            // Step 3: Fetch the policy details
            var policy = await _context.PolicyRegisters
                .FirstOrDefaultAsync(p => p.POLICY_TERM_ID == policyTermId);

            if (policy == null)
            {
                return NotFound($"No policy found for POLICY_TERM_ID: {policyTermId}");
            }

            return Ok(policy);
        }
    }
}
