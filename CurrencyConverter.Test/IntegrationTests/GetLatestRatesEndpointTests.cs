using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CurrencyConverter.Application.DTOs;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyConverter.Test.IntegrationTests;

public class GetLatestRatesEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ReturnsLatestRates_WhenAuthorized()
    {
        var token = GenerateJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/rates/latest?BaseCurrency=USD");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<ExchangeRateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(dto);
        Assert.Equal("USD", dto.Base);
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenTokenMissing()
    {
        var response = await _client.GetAsync("/api/v1/rates/latest?BaseCurrency=USD");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string GenerateJwtToken(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretSigningKey1234567890111"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Role, role),
            new Claim("client_id", "integration-client")
        };

        var token = new JwtSecurityToken(
            issuer: "issuer",
            audience: "audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
