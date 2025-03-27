using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;

namespace BillingPocTwo.Auth.Api.Test
{
    public class AuthControllerTests
    {
        private readonly HttpClient _client;

        public AuthControllerTests()
        {
            var appFactory = new CustomWebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
        }

        [Fact]
        public async Task ChangeUserRole_AdminUser_Success()
        {
            // Arrange
            var userId = Guid.NewGuid(); // Replace with a valid user ID
            var newRole = "Admin";

            // Act
            var response = await _client.PutAsJsonAsync($"/api/auth/change-user-role/{userId}", newRole);

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
