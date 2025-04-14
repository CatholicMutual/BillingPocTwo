using BillingPocTwo.BillingData.Api.Controllers;
using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BillingPocTwo.BillingData.Api.Test
{
    public class EntityAddressControllerTests
    {
        private readonly Mock<IBillingDbContext> _mockContext;
        private readonly Mock<DbSet<ENTITY_ADDRESS_INFO>> _mockDbSet;
        private readonly EntityAddressController _controller;

        public EntityAddressControllerTests()
        {
            _mockContext = new Mock<IBillingDbContext>();
            _mockDbSet = new Mock<DbSet<ENTITY_ADDRESS_INFO>>();

            // Initialize the controller
            _controller = new EntityAddressController(_mockContext.Object);
        }

        private void SetupDbSet(IEnumerable<ENTITY_ADDRESS_INFO> sourceData)
        {
            var queryableData = sourceData.AsQueryable();
            var asyncData = new TestAsyncEnumerable<ENTITY_ADDRESS_INFO>(queryableData);

            _mockDbSet.As<IAsyncEnumerable<ENTITY_ADDRESS_INFO>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(asyncData.GetAsyncEnumerator);

            _mockDbSet.As<IQueryable<ENTITY_ADDRESS_INFO>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<ENTITY_ADDRESS_INFO>(queryableData.Provider));
            _mockDbSet.As<IQueryable<ENTITY_ADDRESS_INFO>>()
                .Setup(m => m.Expression)
                .Returns(queryableData.Expression);
            _mockDbSet.As<IQueryable<ENTITY_ADDRESS_INFO>>()
                .Setup(m => m.ElementType)
                .Returns(queryableData.ElementType);
            _mockDbSet.As<IQueryable<ENTITY_ADDRESS_INFO>>()
                .Setup(m => m.GetEnumerator())
                .Returns(() => queryableData.GetEnumerator());

            _mockContext.Setup(c => c.EntityAddresses).Returns(_mockDbSet.Object);
        }


        [Fact]
        public async Task GetAllEntityAddresses_ShouldReturnOkResult_WithEntityAddresses()
        {
            // Arrange
            var entityAddresses = new List<ENTITY_ADDRESS_INFO>
            {
                new ENTITY_ADDRESS_INFO { SYSTEM_ENTITY_CODE = 1, ADDRESS_TYPE = "Mailing", FULL_NAME = "John Doe" },
                new ENTITY_ADDRESS_INFO { SYSTEM_ENTITY_CODE = 2, ADDRESS_TYPE = "Billing", FULL_NAME = "Jane Smith" }
            }.AsQueryable();

            SetupDbSet(entityAddresses);

            // Act
            var result = await _controller.GetAllEntityAddresses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAddresses = Assert.IsType<List<ENTITY_ADDRESS_INFO>>(okResult.Value);
            Assert.Equal(2, returnedAddresses.Count);
        }

        [Fact]
        public async Task GetEntityAddressesBySystemEntityCode_ShouldReturnOkResult_WhenEntityAddressExists()
        {
            // Arrange
            var entityAddress = new ENTITY_ADDRESS_INFO
            {
                SYSTEM_ENTITY_CODE = 1,
                ADDRESS_TYPE = "Mailing",
                FULL_NAME = "John Doe"
            };

            var entityAddresses = new List<ENTITY_ADDRESS_INFO> { entityAddress };

            SetupDbSet(entityAddresses);

            // Act
            var result = await _controller.GetEntityAddressesBySystemEntityCode(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAddress = Assert.IsType<ENTITY_ADDRESS_INFO>(okResult.Value);
            Assert.Equal(1, returnedAddress.SYSTEM_ENTITY_CODE);
        }

        [Fact]
        public async Task GetEntityAddressesBySystemEntityCode_ShouldReturnNotFound_WhenEntityAddressDoesNotExist()
        {
            // Arrange
            //var entityAddresses = new List<ENTITY_ADDRESS_INFO>
            //{
            //    new ENTITY_ADDRESS_INFO { SYSTEM_ENTITY_CODE = 2, ADDRESS_TYPE = "Billing", FULL_NAME = "Jane Smith" }
            //}.AsQueryable();

            SetupDbSet(new List<ENTITY_ADDRESS_INFO>());

            // Act
            var result = await _controller.GetEntityAddressesBySystemEntityCode(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No addresses found for SYSTEM_ENTITY_CODE: 1", notFoundResult.Value);
        }

        [Fact]
        public async Task GetEntityAddressesBySystemEntityCodes_ShouldReturnOkResult_WithMatchingAddresses()
        {
            // Arrange
            var entityAddresses = new List<ENTITY_ADDRESS_INFO>
            {
                new ENTITY_ADDRESS_INFO { SYSTEM_ENTITY_CODE = 1, ADDRESS_TYPE = "Mailing", FULL_NAME = "John Doe" },
                new ENTITY_ADDRESS_INFO { SYSTEM_ENTITY_CODE = 2, ADDRESS_TYPE = "Billing", FULL_NAME = "Jane Smith" }
            };

            SetupDbSet(entityAddresses);

            // Act
            var result = await _controller.GetEntityAddressesBySystemEntityCodes(new List<decimal> { 1, 2 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAddresses = Assert.IsType<List<ENTITY_ADDRESS_INFO>>(okResult.Value);
            Assert.Equal(2, returnedAddresses.Count);
        }

        [Fact]
        public async Task CreateEntityAddress_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var newEntityAddress = new ENTITY_ADDRESS_INFO
            {
                SYSTEM_ENTITY_CODE = 1,
                ADDRESS_TYPE = "Mailing",
                FULL_NAME = "John Doe"
            };

            _mockDbSet.Setup(m => m.AddAsync(newEntityAddress, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityEntry<ENTITY_ADDRESS_INFO>)null); // Simulate the AddAsync behavio

            _mockContext.Setup(c => c.EntityAddresses).Returns(_mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.CreateEntityAddress(newEntityAddress);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetEntityAddressesBySystemEntityCode), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task UpdateEntityAddress_ShouldReturnNoContent_WhenEntityAddressExists()
        {
            // Arrange
            var existingEntityAddress = new ENTITY_ADDRESS_INFO
            {
                SEQ_ENTITY_ADDRESS_INFO = 1,
                SYSTEM_ENTITY_CODE = 1,
                ADDRESS_TYPE = "Mailing",
                FULL_NAME = "John Doe"
            };

            _mockContext.Setup(c => c.EntityAddresses).Returns(_mockDbSet.Object);
            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(existingEntityAddress);
            _mockDbSet.Setup(m => m.Remove(It.IsAny<ENTITY_ADDRESS_INFO>())).Verifiable();
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateEntityAddress(1, existingEntityAddress);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEntityAddress_ShouldReturnNoContent_WhenEntityAddressExists()
        {
            // Arrange
            var entityAddress = new ENTITY_ADDRESS_INFO
            {
                SEQ_ENTITY_ADDRESS_INFO = 1,
                SYSTEM_ENTITY_CODE = 1,
                ADDRESS_TYPE = "Mailing",
                FULL_NAME = "John Doe"
            };

            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(entityAddress);
            _mockContext.Setup(c => c.EntityAddresses).Returns(_mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteEntityAddress(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEntityAddress_ShouldReturnNotFound_WhenEntityAddressDoesNotExist()
        {
            // Arrange
            _mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((ENTITY_ADDRESS_INFO)null);
            _mockContext.Setup(c => c.EntityAddresses).Returns(_mockDbSet.Object);

            // Act
            var result = await _controller.DeleteEntityAddress(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Entity address with ID 1 not found.", notFoundResult.Value);
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly Expression _expression;

        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
            _expression = Expression.Constant(this);
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
            _expression = expression;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        // Implement the Expression property without using 'override'
        public new Expression Expression => _expression;

        // Implement the ElementType property
        public new Type ElementType => typeof(T);

        // Implement the Provider property
        public new IQueryProvider Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }

    public class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            // Delegate execution to the inner provider
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // Delegate execution to the inner provider
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            // This handles Task<T> results
            var expectedResultType = typeof(TResult);
            if (expectedResultType.IsGenericType && expectedResultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = expectedResultType.GetGenericArguments()[0];

                var result = _inner.Execute(expression); // call the underlying sync Execute
                var taskResult = typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(innerType)
                    .Invoke(null, new[] { result });

                return (TResult)taskResult!;
            }

            // This handles direct TResult returns
            return (TResult)_inner.Execute(expression);
        }
    }
}
