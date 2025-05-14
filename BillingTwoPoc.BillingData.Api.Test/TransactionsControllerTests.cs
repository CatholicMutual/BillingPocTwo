using BillingPocTwo.BillingData.Api.Controllers;
using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BillingPocTwo.BillingData.Api.Test
{
    public class TransactionsControllerTests
    {
        private readonly BillingDbContext _dbContext;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            var options = new DbContextOptionsBuilder<BillingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new BillingDbContext(options);
            _controller = new TransactionsController(_dbContext);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add an entity register
            _dbContext.EntityRegisters.Add(new ENTITY_REGISTER
            {
                SYSTEM_ENTITY_CODE = 1,
                SOURCE_SYSTEM_ENTITY_CODE = "SRC123",
                ENTITY_TYPE = "ACCOUNT"
            });

            // Add policy entity register
            _dbContext.PolicyEntityIntermediate.Add(new POLICY_ENTITY_REGISTER
            {
                POLICY_TERM_ID = 101,
                SYSTEM_ENTITY_CODE = 1,
                SYSTEM_ACTIVITY_NO = "ACT1",
                SYSTEM_TRANSACTION_SEQ = 1001,
                ENTITY_TYPE = "ACCOUNT",
                BILLING_ENTITY_YN = "Y"
            });

            // Add transaction logs
            _dbContext.TransactionLogs.AddRange(new List<TRANSACTION_LOG>
            {
                new TRANSACTION_LOG
                {
                    SYSTEM_TRANSACTION_SEQ = 1001,
                    POLICY_TERM_ID = 101,
                    TRANSACTION_TYPE = "PAYMENT",
                    CREATED_ON = new DateTime(2024, 1, 1),
                    TRANSACTION_EFF_DATE = new DateTime(2024, 1, 1)
                },
                new TRANSACTION_LOG
                {
                    SYSTEM_TRANSACTION_SEQ = 1002,
                    POLICY_TERM_ID = 101,
                    TRANSACTION_TYPE = "INVOICE",
                    CREATED_ON = new DateTime(2024, 2, 1),
                    TRANSACTION_EFF_DATE = new DateTime(2024, 2, 1)
                }
            });

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetTransactionsBySourceSystemEntityCode_ReturnsOk_WithTransactions()
        {
            // Act
            var result = await _controller.GetTransactionsBySourceSystemEntityCode("SRC123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactions = Assert.IsType<List<TRANSACTION_LOG>>(okResult.Value);
            Assert.Equal(2, transactions.Count);
        }

        [Fact]
        public async Task GetTransactionsBySourceSystemEntityCode_ReturnsNotFound_WhenNoEntity()
        {
            var result = await _controller.GetTransactionsBySourceSystemEntityCode("NOTFOUND");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetTransactionsBySourceSystemEntityCode_ReturnsNotFound_WhenNoPolicyTermIds()
        {
            // Add an entity with no policy term
            _dbContext.EntityRegisters.Add(new ENTITY_REGISTER
            {
                SYSTEM_ENTITY_CODE = 2,
                SOURCE_SYSTEM_ENTITY_CODE = "SRC999",
                ENTITY_TYPE = "ACCOUNT"
            });
            _dbContext.SaveChanges();

            var result = await _controller.GetTransactionsBySourceSystemEntityCode("SRC999");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetTransactionsBySourceSystemEntityCode_ReturnsNotFound_WhenNoTransactions()
        {
            // Add a policy entity with no transactions
            _dbContext.EntityRegisters.Add(new ENTITY_REGISTER
            {
                SYSTEM_ENTITY_CODE = 3,
                SOURCE_SYSTEM_ENTITY_CODE = "SRC888",
                ENTITY_TYPE = "ACCOUNT"
            });
            _dbContext.PolicyEntityIntermediate.Add(new POLICY_ENTITY_REGISTER
            {
                POLICY_TERM_ID = 202,
                SYSTEM_ENTITY_CODE = 3,
                SYSTEM_ACTIVITY_NO = "ACT2",
                SYSTEM_TRANSACTION_SEQ = 2001,
                ENTITY_TYPE = "ACCOUNT",
                BILLING_ENTITY_YN = "Y"
            });
            _dbContext.SaveChanges();

            var result = await _controller.GetTransactionsBySourceSystemEntityCode("SRC888");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetTransactionsBySourceSystemEntityCode_ReturnsBadRequest_WhenSourceSystemEntityCodeIsNull()
        {
            var result = await _controller.GetTransactionsBySourceSystemEntityCode(null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTransactionsBySourceSystemEntityCode_AppliesDateFilters()
        {
            // Only the second transaction should match this filter
            var fromDate = new DateTime(2024, 2, 1);
            var result = await _controller.GetTransactionsBySourceSystemEntityCode("SRC123", fromDate);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactions = Assert.IsType<List<TRANSACTION_LOG>>(okResult.Value);
            Assert.Single(transactions);
            Assert.Equal(1002, transactions[0].SYSTEM_TRANSACTION_SEQ);
        }
    }
}
