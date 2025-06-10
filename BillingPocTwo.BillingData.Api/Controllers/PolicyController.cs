using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Route: GET api/Policy/BySourceSystemEntityCodeWithIntermediate/{sourceSystemEntityCode}
        [HttpGet("BySourceSystemEntityCodeWithIntermediate/{sourceSystemEntityCode}")]
        public async Task<IActionResult> GetPoliciesBySourceSystemEntityCodeWithIntermediate(string sourceSystemEntityCode)
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

                // Step 2: Query the POLICY_ENTITY_REGISTER table for POLICY_TERM_IDs associated with the SYSTEM_ENTITY_CODE
                var policyTermIds = await _context.PolicyEntityIntermediate
                    .Where(pe => pe.SYSTEM_ENTITY_CODE == systemEntityCode)
                    .Select(pe => pe.POLICY_TERM_ID)
                    .Distinct()
                    .ToListAsync();

                if (!policyTermIds.Any())
                {
                    return NotFound($"No POLICY_TERM_IDs found for SYSTEM_ENTITY_CODE: {systemEntityCode}");
                }

                // Step 3: Query the POLICY_REGISTER table for policies associated with the POLICY_TERM_IDs
                var policies = await _context.PolicyRegisters
                    .Where(pr => policyTermIds.Contains(pr.POLICY_TERM_ID))
                    .ToListAsync();

                if (!policies.Any())
                {
                    return NotFound($"No policies found for the given POLICY_TERM_IDs.");
                }

                return Ok(policies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        // GET: api/Policy/ByPolicyNoActive/{policyNo}
        [HttpGet("ByPolicyNumActive/{policyNo}")]
        public async Task<IActionResult> GetActivePolicyByPolicyNumber(string policyNo)
        {
            if (string.IsNullOrWhiteSpace(policyNo))
            {
                return BadRequest("POLICY_NO cannot be null or empty.");
            }

            var policies = await _context.PolicyRegisters
                .Where(pr => pr.POLICY_NO == policyNo
                    && pr.SYSTEM_STATUS == "INFORCE"
                    && pr.LEGAL_STATUS != "EXPIRED")
                .ToListAsync();

            if (!policies.Any())
            {
                return NotFound($"No active policies found for POLICY_NO: {policyNo}");
            }

            return Ok(policies);
        }

        // GET: api/Policy/PolicyEntitiesByTermId/{policyTermId}
        [HttpGet("PolicyEntitiesByTermId/{policyTermId}")]
        public async Task<ActionResult<List<POLICY_ENTITY_REGISTER>>> GetPolicyEntitiesByTermId(decimal policyTermId)
        {
            var entities = await _context.PolicyEntityIntermediate
                .Where(pe => pe.POLICY_TERM_ID == policyTermId)
                .ToListAsync();

            if (entities == null || entities.Count == 0)
            {
                return NotFound($"No POLICY_ENTITY_REGISTER entries found for POLICY_TERM_ID: {policyTermId}");
            }

            return Ok(entities);
        }

    }
}
