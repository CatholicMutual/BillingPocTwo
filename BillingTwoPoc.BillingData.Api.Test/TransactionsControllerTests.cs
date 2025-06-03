using BillingPocTwo.BillingData.Api.Controllers;
using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.DataObjects.Billing;
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

            // Add policy register
            _dbContext.PolicyRegisters.Add(new POLICY_REGISTER
            {
                POLICY_TERM_ID = 101,
                POLICY_NO = "POLICY123",
                POLICY_RENEW_NO = 1,
                INBOUND_TRANSACTION_SEQ = 1001,
                PRODUCT_CODE = "PROD",
                STATE_CODE = "ST",
                OPERATING_COMPANY = "OP",
                CRT_CODE = "CRT",
                BROKER_SYSTEM_CODE = 1,
                INSURED_SYSTEM_CODE = 1,
                PAYMENT_PLAN = "PLAN",
                APPLICATION_NO = "APP",
                POLICY_EFFECTIVE_DATE = new DateTime(2024, 1, 1),
                POLICY_EXPIRATION_DATE = new DateTime(2025, 1, 1),
                BILL_TO_SYSTEM_CODE = 1,
                POLICY_ID = 1,
                ROWID = Guid.NewGuid(),
                LEGAL_STATUS = "ACTIVE"
            });

            // Add transaction logs with ACCOUNT_SYSTEM_CODE matching SYSTEM_ENTITY_CODE
            _dbContext.TransactionLogs.AddRange(new List<TRANSACTION_LOG>
            {
                new TRANSACTION_LOG
                {
                    SYSTEM_TRANSACTION_SEQ = 1001,
                    POLICY_TERM_ID = 101,
                    POLICY_NO = "POLICY123",
                    TRANSACTION_TYPE = "PAYMENT",
                    CREATED_ON = new DateTime(2024, 1, 1),
                    TRANSACTION_EFF_DATE = new DateTime(2024, 1, 1),
                    ACCOUNT_SYSTEM_CODE = 1,
                    CREATED_BY = "user1"
                },
                new TRANSACTION_LOG
                {
                    SYSTEM_TRANSACTION_SEQ = 1002,
                    POLICY_TERM_ID = 101,
                    POLICY_NO = "POLICY123",
                    TRANSACTION_TYPE = "INVOICE",
                    CREATED_ON = new DateTime(2024, 2, 1),
                    TRANSACTION_EFF_DATE = new DateTime(2024, 2, 1),
                    ACCOUNT_SYSTEM_CODE = 1,
                    CREATED_BY = "user2"
                }
            });

            _dbContext.PolicyEntityIntermediate.Add(new POLICY_ENTITY_REGISTER
            {
                POLICY_TERM_ID = 101,
                SYSTEM_ENTITY_CODE = 1,
                SYSTEM_ACTIVITY_NO = "ACT1",
                SYSTEM_TRANSACTION_SEQ = 1001,
                ENTITY_TYPE = "ACCOUNT",
                BILLING_ENTITY_YN = "Y"
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
            // Add an entity with no transactions
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

        [Fact]
        public async Task GetTransactionsByAccountSystemCode_ReturnsOk_WithTransactions()
        {
            var result = await _controller.GetTransactionsByAccountSystemCode(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactions = Assert.IsType<List<TRANSACTION_LOG>>(okResult.Value);
            Assert.Equal(2, transactions.Count);
        }

        [Fact]
        public async Task GetTransactionsByAccountSystemCode_ReturnsNotFound_WhenNoTransactions()
        {
            var result = await _controller.GetTransactionsByAccountSystemCode(999);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetTransactionsByAccountSystemCode_ReturnsBadRequest_WhenAccountSystemCodeIsZero()
        {
            var result = await _controller.GetTransactionsByAccountSystemCode(0);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTransactionsByAccountSystemCode_AppliesDateFilters()
        {
            var fromDate = new DateTime(2024, 2, 1);
            var result = await _controller.GetTransactionsByAccountSystemCode(1, fromDate);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactions = Assert.IsType<List<TRANSACTION_LOG>>(okResult.Value);
            Assert.Single(transactions);
            Assert.Equal(1002, transactions[0].SYSTEM_TRANSACTION_SEQ);
        }

        [Fact]
        public async Task GetCombinedTransactions_ReturnsOk_WithData()
        {
            // Arrange: Add an assigned payment for the account and policy
            _dbContext.AssignedPayments.Add(new ASSIGNED_PAYMENT
            {
                PAYMENT_ITEM_SEQ = 1,
                SYSTEM_TRANSACTION_SEQ = 1001,
                TRANSACTION_TYPE = "PAYMENT",
                PAYMENT_AMOUNT = 123.45m,
                CREATED_BY = "tester",
                POLICY_ID = 101,
                ACCOUNT_NO = "SRC123"
            });
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.GetCombinedTransactions("SRC123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<AccountTransactionsDto>(okResult.Value);
            Assert.Equal(2, dto.TransactionLogs.Count); // seeded in setup
            Assert.Single(dto.AssignedPayments);
            Assert.Equal("SRC123", dto.AssignedPayments[0].ACCOUNT_NO);
        }

        [Fact]
        public async Task GetCombinedTransactions_ReturnsNotFound_WhenNoEntity()
        {
            var result = await _controller.GetCombinedTransactions("NOTFOUND");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCombinedTransactions_ReturnsOk_WithEmptyLists_WhenNoTransactionsOrPayments()
        {
            // Add an entity and policy, but no transactions or payments
            _dbContext.EntityRegisters.Add(new ENTITY_REGISTER
            {
                SYSTEM_ENTITY_CODE = 4,
                SOURCE_SYSTEM_ENTITY_CODE = "SRCEMPTY",
                ENTITY_TYPE = "ACCOUNT"
            });
            _dbContext.PolicyEntityIntermediate.Add(new POLICY_ENTITY_REGISTER
            {
                POLICY_TERM_ID = 303,
                SYSTEM_ENTITY_CODE = 4,
                SYSTEM_ACTIVITY_NO = "ACT3",
                SYSTEM_TRANSACTION_SEQ = 3001,
                ENTITY_TYPE = "ACCOUNT",
                BILLING_ENTITY_YN = "Y"
            });
            _dbContext.SaveChanges();

            var result = await _controller.GetCombinedTransactions("SRCEMPTY");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<AccountTransactionsDto>(okResult.Value);
            Assert.Empty(dto.TransactionLogs);
            Assert.Empty(dto.AssignedPayments);
        }

        [Fact]
        public async Task GetCombinedTransactions_ReturnsBadRequest_WhenSourceSystemEntityCodeIsNull()
        {
            var result = await _controller.GetCombinedTransactions(null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCombinedTransactions_AppliesDateFilters()
        {
            // Add an assigned payment with a specific date (simulate by SYSTEM_TRANSACTION_SEQ)
            _dbContext.AssignedPayments.Add(new ASSIGNED_PAYMENT
            {
                PAYMENT_ITEM_SEQ = 2,
                SYSTEM_TRANSACTION_SEQ = 1002,
                TRANSACTION_TYPE = "PAYMENT",
                PAYMENT_AMOUNT = 200m,
                CREATED_BY = "tester2",
                POLICY_ID = 101,
                ACCOUNT_NO = "SRC123"
            });
            _dbContext.SaveChanges();

            var fromDate = new DateTime(2024, 2, 1);
            var result = await _controller.GetCombinedTransactions("SRC123", fromDate);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<AccountTransactionsDto>(okResult.Value);
            Assert.Single(dto.TransactionLogs); // Only the second transaction matches
            Assert.Single(dto.AssignedPayments); // All assigned payments are returned (no date filter on payments)
        }

    }
}
