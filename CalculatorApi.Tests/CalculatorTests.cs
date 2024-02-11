using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CalculatorAPI.Tests
{
    public class CalculatorApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CalculatorApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/token")]
        [InlineData("/api/calculate")]
        public async Task Unauthorized_Access_Endpoints_Without_Token_Should_Return_Unauthorized(string endpoint)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync(endpoint, null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private async Task<string> GetTokenAsync()
        {
            // Arrange
            var client = _factory.CreateClient();
            var clientId = "sampleClientId";
            var clientSecret = "sampleClientSecret";

            var formData = new MultipartFormDataContent
            {
                { new StringContent(clientId), "clientId" },
                { new StringContent(clientSecret), "clientSecret" }
            };

            // Act
            var response = await client.PostAsync("/api/token", formData);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();
            return token;

        }
        [Fact]
        public async Task Token_Endpoint_Should_Return_Valid_Token_With_Correct_Credentials()
        {           

            var token = await GetTokenAsync();

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Calculate_Endpoint_Should_Return_Correct_Result()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await GetTokenAsync();

            var request = new CalculatorRequest (10, 5 );
            var operation = "add";
            
            client.DefaultRequestHeaders.Add("operation", operation);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.PostAsync("/api/calculate", 
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(content);

            // Assert
            result.Should().NotBeNull();
            ((double)result.result).Should().Be(15);
        }
    }
}
