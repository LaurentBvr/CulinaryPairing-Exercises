using System.Net.Http.Json;

namespace CulinaryPairing.Pwa.Services;

public class DishApiService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public DishApiService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private void SetAuthHeader()
    {
        if (_auth.Token != null)
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
    }

    public async Task<List<DishHeader>> GetAllAsync()
    {
        SetAuthHeader();
        try
        {
            return await _http.GetFromJsonAsync<List<DishHeader>>("dishes")
                ?? new List<DishHeader>();
        }
        catch (HttpRequestException)
        {
            return new List<DishHeader>();
        }
    }

    public async Task<DishDetailDto?> GetByIdAsync(Guid id)
    {
        SetAuthHeader();
        try
        {
            return await _http.GetFromJsonAsync<DishDetailDto>($"dishes/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(CreateDishRequest request)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.PostAsJsonAsync("dishes", request);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<List<PairingHeader>> GetPairingsAsync(Guid dishId)
    {
        SetAuthHeader();
        try
        {
            return await _http.GetFromJsonAsync<List<PairingHeader>>($"dishes/{dishId}/pairings")
                ?? new List<PairingHeader>();
        }
        catch (HttpRequestException)
        {
            return new List<PairingHeader>();
        }
    }

    public async Task<bool> CreatePairingAsync(Guid dishId, CreatePairingRequest request)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.PostAsJsonAsync($"dishes/{dishId}/pairings", request);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> ValidatePairingAsync(Guid pairingId)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.PutAsync($"dishes/pairings/{pairingId}/validate", null);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}

public record DishHeader(Guid Id, string Name, int IngredientCount);
public record DishDetailDto(Guid Id, string Name, string? Description, DateTime CreatedAt, List<IngredientModel> Ingredients);
public record IngredientModel(Guid Id, string Name, string Category);
public record PairingHeader(Guid Id, string BeverageName, int Score, bool IsValidated);
public record CreateDishRequest(Guid DishId, string Name);
public record CreatePairingRequest(string BeverageName, int Score);