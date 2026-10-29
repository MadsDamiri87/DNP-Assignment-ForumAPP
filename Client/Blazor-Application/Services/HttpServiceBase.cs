using System.Net.Http.Json;
using System.Text.Json;

namespace Blazor_Application.Services;

public abstract class HttpServiceBase<TDto, TCreate, TUpdate> : IHttpService<TDto, TCreate, TUpdate>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient client;
    private readonly string resource;

    protected HttpServiceBase(HttpClient client, string resource)
    {
        this.client = client;
        this.resource = resource;
    }

    public Task<TDto> AddAsync(TCreate request) =>
        SendPostAsync<TDto>(resource, request!);

    public Task<TDto> UpdateAsync(int id, TUpdate request) =>
        SendPutAsync<TDto>($"{resource}/{id}", request!);

    public Task DeleteAsync(int id) =>
        SendDeleteAsync($"{resource}/{id}");

    public Task<TDto> GetSingleAsync(int id) =>
        SendGetAsync<TDto>($"{resource}/{id}");

    protected Task<List<TDto>> GetManyAsync(params (string Name, object? Value)[] filters) =>
        SendGetAsync<List<TDto>>(BuildUri(resource, filters));

    protected async Task<TResponse> SendPostAsync<TResponse>(string uri, object request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync(uri, request);
        return Deserialize<TResponse>(await ReadSuccessContentAsync(httpResponse));
    }

    protected async Task<TResponse> SendPutAsync<TResponse>(string uri, object request)
    {
        HttpResponseMessage httpResponse = await client.PutAsJsonAsync(uri, request);
        return Deserialize<TResponse>(await ReadSuccessContentAsync(httpResponse));
    }

    protected async Task<TResponse> SendGetAsync<TResponse>(string uri)
    {
        HttpResponseMessage httpResponse = await client.GetAsync(uri);
        return Deserialize<TResponse>(await ReadSuccessContentAsync(httpResponse));
    }

    protected async Task SendDeleteAsync(string uri)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync(uri);
        await ReadSuccessContentAsync(httpResponse);
    }

    protected static string BuildUri(string path, params (string Name, object? Value)[] filters)
    {
        List<string> queryParts = [];

        foreach ((string name, object? value) in filters)
        {
            string? text = value?.ToString();

            if (!string.IsNullOrWhiteSpace(text))
            {
                queryParts.Add($"{name}={Uri.EscapeDataString(text)}");
            }
        }

        return queryParts.Count == 0 ? path : $"{path}?{string.Join('&', queryParts)}";
    }

    private static async Task<string> ReadSuccessContentAsync(HttpResponseMessage httpResponse)
    {
        string content = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestException(string.IsNullOrWhiteSpace(content)
                ? $"{(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}"
                : content);
        }

        return content;
    }

    private static TResponse Deserialize<TResponse>(string content) =>
        JsonSerializer.Deserialize<TResponse>(content, JsonOptions)!;
}
