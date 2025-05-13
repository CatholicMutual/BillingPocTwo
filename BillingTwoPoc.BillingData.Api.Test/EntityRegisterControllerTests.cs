using BillingPocTwo.BillingData.Api.Controllers;
using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.DataObjects.Billing;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BillingPocTwo.BillingData.Api.Test
{
    public class EntityRegisterControllerTests
    {
        private readonly Mock<IBillingDbContext> _mockDbContext;
        private readonly EntityRegisterController _controller;

        public EntityRegisterControllerTests()
        {
            _mockDbContext = new Mock<IBillingDbContext>();
            _controller = new EntityRegisterController(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllEntityRegisters_ReturnsOkResult_WithEntityRegisters()
        {
            // Arrange
            var mockEntityRegisters = new List<ENTITY_REGISTER>
    {
        new ENTITY_REGISTER { SYSTEM_ENTITY_CODE = 1, ENTITY_TYPE = "ACCOUNT", DOING_BUSINESS_AS_NAME = "Test Account 1" },
        new ENTITY_REGISTER { SYSTEM_ENTITY_CODE = 2, ENTITY_TYPE = "ACCOUNT", DOING_BUSINESS_AS_NAME = "Test Account 2" }
    };

            _mockDbContext.Setup(c => c.EntityRegisters).Returns(MockDbSet(mockEntityRegisters).Object);

            // Act
            var result = await _controller.GetAllEntityRegisters();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ENTITY_REGISTER>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetEntityDetails_ReturnsNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            _mockDbContext.Setup(c => c.EntityRegisters)
                .Returns(MockDbSet(new List<ENTITY_REGISTER>()).Object);

            // Act
            var result = await _controller.GetEntityDetails("NonExistentCode");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetEntityDetails_ReturnsOkResult_WithEntityDetails()
        {
            // Arrange
            var mockEntityRegisters = new List<ENTITY_REGISTER>
            {
                new ENTITY_REGISTER { SYSTEM_ENTITY_CODE = 1, SOURCE_SYSTEM_ENTITY_CODE = "TestCode", ENTITY_TYPE = "ACCOUNT" }
            }.AsQueryable();

            var mockEntityAddresses = new List<ENTITY_ADDRESS_INFO>
            {
                new ENTITY_ADDRESS_INFO { SYSTEM_ENTITY_CODE = 1, FULL_NAME = "Test Address" }
            }.AsQueryable();

            var mockPolicyEntityRegisters = new List<POLICY_ENTITY_REGISTER>
            {
                new POLICY_ENTITY_REGISTER { SYSTEM_ENTITY_CODE = 1, POLICY_TERM_ID = 123 }
            }.AsQueryable();

            _mockDbContext.Setup(c => c.EntityRegisters).Returns(MockDbSet(mockEntityRegisters).Object);
            _mockDbContext.Setup(c => c.EntityAddresses).Returns(MockDbSet(mockEntityAddresses).Object);
            _mockDbContext.Setup(c => c.PolicyEntityIntermediate).Returns(MockDbSet(mockPolicyEntityRegisters).Object);

            // Act
            var result = await _controller.GetEntityDetails("TestCode");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AccountDetailsDto>(okResult.Value);
            Assert.Equal("TestCode", returnValue.SOURCE_SYSTEM_ENTITY_CODE);
            Assert.Equal("Test Address", returnValue.FULL_NAME);
            Assert.Single(returnValue.PolicyTermIds);
        }

        private Mock<DbSet<T>> MockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();

            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Support for IAsyncEnumerable
            mockDbSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            mockDbSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

            return mockDbSet;
        }
    }
}
