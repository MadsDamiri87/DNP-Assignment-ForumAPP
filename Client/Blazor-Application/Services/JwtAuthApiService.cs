using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using ApiContracts;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Blazor_Application.Services;

public class JwtAuthApiService(HttpClient httpClient, ProtectedLocalStorage storage) : IAuthApiService
{
    private const string StorageKey = "jwt";
    public string? Jwt { get; private set; }

    public event Action<ClaimsPrincipal>? OnAuthStateChanged;
    
    public async Task LoginAsync(string userName, string password)
    {
        AuthDto request = new AuthDto
        {
            UserName = userName,
            Password = password
        };
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("auth/login", request);
        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(content);
        }

        await storage.SetAsync(StorageKey, content);

        ClaimsPrincipal principal = UseToken(content);
        OnAuthStateChanged?.Invoke(principal);
    }

    public async Task LogoutAsync()
    {
        await storage.DeleteAsync(StorageKey);
        ClaimsPrincipal principal = UseToken(null);
        OnAuthStateChanged?.Invoke(principal);
    }

    private ClaimsPrincipal UseToken(string? token)
    {
        List<Claim> claims = token is null ? [] : ParseClaimsFromJwt(token);

        if (claims.Count == 0 || IsExpired(claims))
        {
            Jwt = null;
            httpClient.DefaultRequestHeaders.Authorization = null;
            return new ClaimsPrincipal();
        }

        Jwt = token;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        ClaimsIdentity identity = new ClaimsIdentity(claims, authenticationType: "jwt",
            nameType: "Username", roleType: "Role");

        return new ClaimsPrincipal(identity);
    }

    private static List<Claim> ParseClaimsFromJwt(string jwt)
    {
        string payload = jwt.Split('.')[1];
        byte[] json = Base64Url.DecodeFromChars(payload);
        Dictionary<string, JsonElement> values =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
        return values.Select(pair => new Claim(pair.Key, pair.Value.ToString())).ToList();
    }

    private static bool IsExpired(List<Claim> claims)
    {
        Claim? exp = claims.FirstOrDefault(claim => claim.Type == "exp");

        if (exp is null)
        {
            return true;
        }

        return DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp.Value)) <
               DateTimeOffset.UtcNow;
    }

    public async Task<ClaimsPrincipal> GetAuthAsync()
    {
        Jwt ??= await LoadTokenFromStorageAsync();
        
        return UseToken(Jwt);
    }
    
    private async Task<string?> LoadTokenFromStorageAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<string> result = await storage.GetAsync<string>(
                StorageKey);
            return result.Success ? result.Value : null;
        }
        catch (CryptographicException)
        {
            await storage.DeleteAsync(StorageKey);
            return null;
        }
    }
    
}