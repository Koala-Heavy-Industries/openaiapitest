using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenAIApiClient.Models;

namespace OpenAIApiClient;

public class OpenAIApiService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenAIApiService(string baseUrl = "http://172.19.96.1:1234", string? apiKey = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<ModelListResponse> GetModelsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/v1/models");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ModelListResponse>(json, _jsonOptions) ?? new ModelListResponse();
    }

    public async Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_baseUrl}/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, _jsonOptions) ?? new ChatCompletionResponse();
    }

    public async Task<string> SimpleChatAsync(string message, string model = "google/gemma-3n-e4b")
    {
        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = message }
            },
            MaxTokens = 500,
            Temperature = 0.7
        };

        var response = await CreateChatCompletionAsync(request);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "No response";
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}