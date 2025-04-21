using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Test.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CurrencyConverter.Test.IntegrationTests;

public class GetHistoricalRatesEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ReturnsHistoricalRates_WhenAuthorized()
    {
        // Arrange
        var token = JwtGenerator.GenerateJwtToken("viewer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var query = "?BaseCurrency=USD&StartDate=2024-03-01&EndDate=2024-03-05";
        var response = await _client.GetAsync("/api/rates/historical/v1" + query);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<ListExchangeRateDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(dto);
        Assert.Equal("USD", dto.Items?.FirstOrDefault()?.Base);
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenNoToken()
    {
        var query = "?BaseCurrency=USD&StartDate=2024-03-01&EndDate=2024-03-05";
        var response = await _client.GetAsync("/api/rates/historical/v1" + query);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public record TestExchangeRateDto(
        [property: JsonPropertyName("base")] string Base,
        [property: JsonPropertyName("date")] DateTime Date,
        [property: JsonPropertyName("rates")] Dictionary<string, decimal> Rates,
        [property: JsonPropertyName("historicalRates")] 
        Dictionary<DateTime, Dictionary<string, decimal>>? HistoricalRates = null
);

    public record ListExchangeRateDto(List<ExchangeRateDto> Items);
}
