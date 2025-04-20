using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Test.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyConverter.Test.IntegrationTests;

public class GetHistoricalRatesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetHistoricalRatesEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ReturnsHistoricalRates_WhenAuthorized()
    {
        // Arrange
        var token = JwtGenerator.GenerateJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var query = "?BaseCurrency=USD&StartDate=2024-03-01&EndDate=2024-03-05";
        var response = await _client.GetAsync("/api/rates/historical" + query);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<ExchangeRateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(dto);
        Assert.Equal("USD", dto.Base);
        Assert.NotNull(dto.HistoricalRates);
        Assert.True(dto.HistoricalRates!.Count > 0);
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenNoToken()
    {
        var query = "?BaseCurrency=USD&StartDate=2024-03-01&EndDate=2024-03-05";
        var response = await _client.GetAsync("/api/rates/historical" + query);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
