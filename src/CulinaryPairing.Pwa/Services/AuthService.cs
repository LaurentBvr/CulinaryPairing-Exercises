using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace CulinaryPairing.Pwa.Services;

public class AuthService : AuthenticationStateProvider
{
    private readonly HttpClient _http;
    private string? _token;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public AuthService(HttpClient http) => _http = http;

    public string? Token => _token;
    public bool IsAuthenticated => _token != null;

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login",
                new { Email = email, Password = password });

            if (!response.IsSuccessStatusCode)
                return new LoginResult(false, "Email ou mot de passe incorrect");

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result?.Token == null)
                return new LoginResult(false, "Erreur serveur");

            _token = result.Token;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return new LoginResult(true, null, result.FullName, result.Roles);
        }
        catch (Exception ex)
        {
            return new LoginResult(false, $"Erreur de connexion: {ex.Message}");
        }
    }

    public async Task<RegisterResult> RegisterAsync(
        string email, string password, string firstName, string lastName)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register",
                new { Email = email, Password = password, FirstName = firstName, LastName = lastName });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new RegisterResult(false, error);
            }

            return new RegisterResult(true, null);
        }
        catch (Exception ex)
        {
            return new RegisterResult(false, ex.Message);
        }
    }

    public void Logout()
    {
        _token = null;
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_token == null)
            return Task.FromResult(new AuthenticationState(_anonymous));

        var claims = ParseClaimsFromJwt(_token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var kvp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;

        var claims = new List<Claim>();
        foreach (var kv in kvp)
        {
            if (kv.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in kv.Value.EnumerateArray())
                    claims.Add(new Claim(kv.Key, item.GetString()!));
            }
            else
            {
                claims.Add(new Claim(kv.Key, kv.Value.ToString()));
            }
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

public record LoginResult(bool Success, string? Error, string? FullName = null, List<string>? Roles = null);
public record RegisterResult(bool Success, string? Error);
public record LoginResponse(string Token, string Email, string FullName, List<string> Roles);