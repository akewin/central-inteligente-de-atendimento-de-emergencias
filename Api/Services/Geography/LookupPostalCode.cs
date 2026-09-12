using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Api.Dtos;
using Api.Middlewares;

namespace Api.Services.Geography;

public sealed class LookupPostalCode
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LookupPostalCode(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<PostalCodeAddressDto> ExecuteAsync(string? postalCode, CancellationToken cancellationToken)
    {
        var digits = new string((postalCode ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length != 8)
            throw new AppException("O CEP deve possuir 8 dígitos.");

        try
        {
            var client = _httpClientFactory.CreateClient("ViaCep");
            using var response = await client.GetAsync($"ws/{digits}/json/", cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new AppException("Não foi possível consultar o CEP no momento.", (int)HttpStatusCode.BadGateway);

            var result = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: cancellationToken);
            if (result is null || result.HasError || string.IsNullOrWhiteSpace(result.State))
                throw new AppException("CEP não encontrado.", (int)HttpStatusCode.NotFound);

            return new PostalCodeAddressDto
            {
                PostalCode = digits,
                Street = result.Street?.Trim() ?? string.Empty,
                Neighborhood = result.Neighborhood?.Trim() ?? string.Empty,
                City = result.City?.Trim() ?? string.Empty,
                State = result.State.Trim().ToUpperInvariant()
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AppException("A consulta do CEP excedeu o tempo limite.", (int)HttpStatusCode.GatewayTimeout);
        }
        catch (HttpRequestException)
        {
            throw new AppException("O serviço de consulta de CEP está indisponível.", (int)HttpStatusCode.BadGateway);
        }
    }

    private sealed class ViaCepResponse
    {
        [JsonPropertyName("logradouro")] public string? Street { get; set; }
        [JsonPropertyName("bairro")] public string? Neighborhood { get; set; }
        [JsonPropertyName("localidade")] public string? City { get; set; }
        [JsonPropertyName("uf")] public string State { get; set; } = string.Empty;
        [JsonPropertyName("erro")] public JsonElement Error { get; set; }

        [JsonIgnore]
        public bool HasError => Error.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => bool.TryParse(Error.GetString(), out var value) && value,
            _ => false
        };
    }
}
