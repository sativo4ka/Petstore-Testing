using System.Net;
using System.Net.Http.Json;

namespace PetstoreTests;

public static class ApiTestHelper
{
    public const string BaseUrl = "https://petstore.swagger.io/v2/";

    public static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        return client;
    }

    public static long NewId() =>
        Random.Shared.NextInt64(100_000_000, 2_000_000_000);

    public static string NewName(string prefix) =>
        $"{prefix}_{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 13, prefix.Length + 33)];

    public static async Task<HttpResponseMessage> CreatePet(HttpClient client, long? id = null, string? status = null)
    {
        var petId = id ?? NewId();
        return await client.PostAsJsonAsync("pet", new
        {
            id = petId,
            name = NewName("pet"),
            photoUrls = new[] { "https://example.com/photo.jpg" },
            status = status ?? "available"
        });
    }

    public static async Task<HttpResponseMessage> CreateOrder(HttpClient client, int? id = null, int quantity = 1, string status = "placed")
    {
        return await client.PostAsJsonAsync("store/order", new
        {
            id = id ?? (int)(NewId() % 9_000_000 + 1),
            petId = 1,
            quantity,
            shipDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            status,
            complete = false
        });
    }

    public static async Task<HttpResponseMessage> CreateUser(HttpClient client, string? username = null)
    {
        var name = username ?? NewName("user");
        return await client.PostAsJsonAsync("user", new
        {
            id = NewId(),
            username = name,
            firstName = "Test",
            lastName = "User",
            email = $"{name}@example.com",
            password = "TestPass123",
            phone = "123456789",
            userStatus = 1
        });
    }

    public static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void CheckStatus(HttpResponseMessage response, params HttpStatusCode[] allowed)
    {
        Check(allowed.Contains(response.StatusCode),
            $"Unexpected HTTP status: {(int)response.StatusCode} {response.StatusCode}. Body: {awaitBody(response)}");
    }

    private static string awaitBody(HttpResponseMessage response)
    {
        try { return response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); }
        catch { return "<response body unavailable>"; }
    }

    public static async Task DeletePet(HttpClient client, long id)
    {
        try { await client.DeleteAsync($"pet/{id}"); } catch { }
    }

    public static async Task DeleteOrder(HttpClient client, int id)
    {
        try { await client.DeleteAsync($"store/order/{id}"); } catch { }
    }

    public static async Task DeleteUser(HttpClient client, string username)
    {
        try { await client.DeleteAsync($"user/{Uri.EscapeDataString(username)}"); } catch { }
    }
}
