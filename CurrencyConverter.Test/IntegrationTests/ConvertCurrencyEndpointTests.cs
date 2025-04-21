using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Test.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CurrencyConverter.Test.IntegrationTests;

public class ConvertCurrencyEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ConvertsCurrency_WhenAuthorized()
    {
        var token = JwtGenerator.GenerateJwtToken("admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ConversionRequestDto("USD", "EUR", 100);
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/rates/convert/v1", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<ConversionResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(dto);
        Assert.Equal("USD", dto!.FromCurrency);
        Assert.Equal("EUR", dto.ToCurrency);
        Assert.Equal(100, dto.OriginalAmount);
        Assert.True(dto.ConvertedAmount > 0);
    }

   
}
