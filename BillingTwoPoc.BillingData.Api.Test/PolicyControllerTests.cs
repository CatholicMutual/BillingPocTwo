using BillingPocTwo.BillingData.Api.Controllers;
using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BillingPocTwo.BillingData.Api.Test
{
    public class PolicyControllerTests
    {
        private readonly BillingDbContext _dbContext;
        private readonly PolicyController _controller;

        public PolicyControllerTests()
        {
            // Set up the in-memory database
            var options = new DbContextOptionsBuilder<BillingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new BillingDbContext(options);
            _controller = new PolicyController(_dbContext);

            // Seed the database with test data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add test data for POLICY_ENTITY_REGISTER
            _dbContext.PolicyEntityIntermediate.AddRange(new List<POLICY_ENTITY_REGISTER>
            {
                new POLICY_ENTITY_REGISTER
                {
                    SYSTEM_ENTITY_CODE = 1,
                    POLICY_TERM_ID = 101,
                    SYSTEM_ACTIVITY_NO = "ACT101",
                    SYSTEM_TRANSACTION_SEQ = 1001,
                    ENTITY_TYPE = "ACCOUNT",
                    BILLING_ENTITY_YN = "Y"
                },
                new POLICY_ENTITY_REGISTER
                {
                    SYSTEM_ENTITY_CODE = 1,
                    POLICY_TERM_ID = 102,
                    SYSTEM_ACTIVITY_NO = "ACT102",
                    SYSTEM_TRANSACTION_SEQ = 1002,
                    ENTITY_TYPE = "ACCOUNT",
                    BILLING_ENTITY_YN = "N"
                    }
            });

            // Add test data for POLICY_REGISTER
            _dbContext.PolicyRegisters.AddRange(new List<POLICY_REGISTER>
            {
                new POLICY_REGISTER
                {
                    POLICY_TERM_ID = 101,
                    POLICY_NO = "POL101",
                    APPLICATION_NO = "APP101",
                    CRT_CODE = "CRT1",
                    LEGAL_STATUS = "Active",
                    OPERATING_COMPANY = "COMP1",
                    PAYMENT_PLAN = "Monthly",
                    PRODUCT_CODE = "PROD1",
                    STATE_CODE = "NY",
                    POLICY_RENEW_NO = 0,
                    INBOUND_TRANSACTION_SEQ = 0,
                    BROKER_SYSTEM_CODE = 0,
                    INSURED_SYSTEM_CODE = 0,
                    POLICY_EFFECTIVE_DATE = DateTime.UtcNow,
                    POLICY_EXPIRATION_DATE = DateTime.UtcNow.AddYears(1),
                    BILL_TO_SYSTEM_CODE = 0,
                    POLICY_ID = 101,
                    ROWID = Guid.NewGuid()
                },
                new POLICY_REGISTER
                {
                    POLICY_TERM_ID = 102,
                    POLICY_NO = "POL102",
                    APPLICATION_NO = "APP102",
                    CRT_CODE = "CRT2",
                    LEGAL_STATUS = "Active",
                    OPERATING_COMPANY = "COMP2",
                    PAYMENT_PLAN = "Quarterly",
                    PRODUCT_CODE = "PROD2",
                    STATE_CODE = "CA",
                    POLICY_RENEW_NO = 0,
                    INBOUND_TRANSACTION_SEQ = 0,
                    BROKER_SYSTEM_CODE = 0,
                    INSURED_SYSTEM_CODE = 0,
                    POLICY_EFFECTIVE_DATE = DateTime.UtcNow,
                    POLICY_EXPIRATION_DATE = DateTime.UtcNow.AddYears(1),
                    BILL_TO_SYSTEM_CODE = 0,
                    POLICY_ID = 102,
                    ROWID = Guid.NewGuid()
                    }
            });

            //// Add test data for POLICY_REGISTER
            //_dbContext.PolicyRegisters.AddRange(new List<POLICY_REGISTER>
            //{
            //    new POLICY_REGISTER { POLICY_TERM_ID = 101, POLICY_NO = "POL101" },
            //    new POLICY_REGISTER { POLICY_TERM_ID = 102, POLICY_NO = "POL102" }
            //});

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetPoliciesBySystemEntityCode_ReturnsOkResult_WithPolicies()
        {
            // Act
            var result = await _controller.GetPoliciesBySystemEntityCode(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<POLICY_REGISTER>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetPoliciesBySystemEntityCode_ReturnsNotFound_WhenNoPoliciesExist()
        {
            // Act
            var result = await _controller.GetPoliciesBySystemEntityCode(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetPolicyDetails_ReturnsOkResult_WithPolicyDetails()
        {
            // Arrange
            _dbContext.EntityRegisters.Add(new ENTITY_REGISTER
            {
                SYSTEM_ENTITY_CODE = 1,
                SOURCE_SYSTEM_ENTITY_CODE = "TestCode",
                ENTITY_TYPE = "ACCOUNT"
            });
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.GetPolicyDetails("TestCode", 101);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<POLICY_REGISTER>(okResult.Value);
            Assert.Equal("POL101", returnValue.POLICY_NO);
        }

        [Fact]
        public async Task GetPolicyDetails_ReturnsNotFound_WhenPolicyDoesNotExist()
        {
            // Arrange
            _dbContext.EntityRegisters.Add(new ENTITY_REGISTER
            {
                SYSTEM_ENTITY_CODE = 1,
                SOURCE_SYSTEM_ENTITY_CODE = "TestCode",
                ENTITY_TYPE = "ACCOUNT"
            });
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.GetPolicyDetails("TestCode", 999);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);

        }
    }
}
