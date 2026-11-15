using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace PetstoreTests;

[TestFixture]
public class PetstoreApiTests
{
    [Test]
    public async Task Pet_Create_And_Read()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        var create = await ApiTestHelper.CreatePet(c, id);
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var json = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(json.GetProperty("id").GetInt64() == id, "Returned pet id differs.");
        await ApiTestHelper.DeletePet(c, id);
    }

    [Test]
    public async Task Pet_Create_With_Available_Status()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        var create = await ApiTestHelper.CreatePet(c, id, "available");
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("status").GetString() == "available", "Status is not available.");
        await ApiTestHelper.DeletePet(c, id);
    }

    [Test]
    public async Task Pet_Create_With_Pending_Status()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        var create = await ApiTestHelper.CreatePet(c, id, "pending");
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("status").GetString() == "pending", "Status is not pending.");
        await ApiTestHelper.DeletePet(c, id);
    }

    [Test]
    public async Task Pet_Create_With_Sold_Status()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        var create = await ApiTestHelper.CreatePet(c, id, "sold");
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("status").GetString() == "sold", "Status is not sold.");
        await ApiTestHelper.DeletePet(c, id);
    }

    [Test]
    public async Task Pet_Update_Existing_Record()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        await ApiTestHelper.CreatePet(c, id);
        var update = await c.PutAsJsonAsync("pet", new { id, name = "updated_pet", photoUrls = new[] { "https://example.com/new.jpg" }, status = "pending" });
        ApiTestHelper.CheckStatus(update, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("name").GetString() == "updated_pet", "Pet name was not updated.");
        await ApiTestHelper.DeletePet(c, id);
    }

    [Test]
    public async Task Pet_Delete_Existing_Record()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        await ApiTestHelper.CreatePet(c, id);
        var delete = await c.DeleteAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(delete, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Pet_Get_Nonexistent_Record()
    {
        using var c = ApiTestHelper.CreateClient();
        var get = await c.GetAsync($"pet/{ApiTestHelper.NewId()}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Pet_Delete_Nonexistent_Record()
    {
        using var c = ApiTestHelper.CreateClient();
        var delete = await c.DeleteAsync($"pet/{ApiTestHelper.NewId()}");
        ApiTestHelper.CheckStatus(delete, HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Test]
    public async Task Pet_Find_By_Available_Status()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("pet/findByStatus?status=available");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        ApiTestHelper.Check(body.TrimStart().StartsWith("["), "Expected a JSON array.");
    }

    [Test]
    public async Task Pet_Find_By_Pending_Status()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("pet/findByStatus?status=pending");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        ApiTestHelper.Check(body.TrimStart().StartsWith("["), "Expected a JSON array.");
    }

    [Test]
    public async Task Pet_Find_By_Sold_Status()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("pet/findByStatus?status=sold");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        ApiTestHelper.Check(body.TrimStart().StartsWith("["), "Expected a JSON array.");
    }

    [Test]
    public async Task Pet_Find_By_Tag()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("pet/findByTags?tags=regression");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        ApiTestHelper.Check(body.TrimStart().StartsWith("["), "Expected a JSON array.");
    }

    [Test]
    public async Task Pet_Form_Update_Existing_Record()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = ApiTestHelper.NewId();
        await ApiTestHelper.CreatePet(c, id);
        var form = new FormUrlEncodedContent(new[] { new KeyValuePair<string,string>("name", "form_updated"), new KeyValuePair<string,string>("status", "pending") });
        var update = await c.PostAsync($"pet/{id}", form);
        ApiTestHelper.CheckStatus(update, HttpStatusCode.OK);
        var get = await c.GetAsync($"pet/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        await ApiTestHelper.DeletePet(c, id);
    }

    [Test]
    public async Task Store_Get_Inventory()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("store/inventory");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.ValueKind == System.Text.Json.JsonValueKind.Object, "Inventory must be a JSON object.");
    }

    [Test]
    public async Task Store_Create_And_Read_Order()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = (int)(ApiTestHelper.NewId() % 9_000_000 + 1);
        var create = await ApiTestHelper.CreateOrder(c, id);
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"store/order/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("id").GetInt32() == id, "Order id differs.");
        await ApiTestHelper.DeleteOrder(c, id);
    }

    [Test]
    public async Task Store_Create_Order_Quantity_Two()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = (int)(ApiTestHelper.NewId() % 9_000_000 + 1);
        var create = await ApiTestHelper.CreateOrder(c, id, 2);
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"store/order/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("quantity").GetInt32() == 2, "Order quantity differs.");
        await ApiTestHelper.DeleteOrder(c, id);
    }

    [Test]
    public async Task Store_Create_Approved_Order()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = (int)(ApiTestHelper.NewId() % 9_000_000 + 1);
        var create = await ApiTestHelper.CreateOrder(c, id, 1, "approved");
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"store/order/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("status").GetString() == "approved", "Order status differs.");
        await ApiTestHelper.DeleteOrder(c, id);
    }

    [Test]
    public async Task Store_Delete_Existing_Order()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = (int)(ApiTestHelper.NewId() % 9_000_000 + 1);
        await ApiTestHelper.CreateOrder(c, id);
        var delete = await c.DeleteAsync($"store/order/{id}");
        ApiTestHelper.CheckStatus(delete, HttpStatusCode.OK);
        var get = await c.GetAsync($"store/order/{id}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Store_Get_Nonexistent_Order()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync($"store/order/{(int)(ApiTestHelper.NewId() % 9_000_000 + 1)}");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Store_Delete_Nonexistent_Order()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.DeleteAsync($"store/order/{(int)(ApiTestHelper.NewId() % 9_000_000 + 1)}");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Test]
    public async Task Store_Order_Id_Zero()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("store/order/0");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Store_Create_Second_Order()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = (int)(ApiTestHelper.NewId() % 9_000_000 + 1);
        var create = await ApiTestHelper.CreateOrder(c, id, 3, "placed");
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var body = await create.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("quantity").GetInt32() == 3, "Quantity must be 3.");
        await ApiTestHelper.DeleteOrder(c, id);
    }

    [Test]
    public async Task Store_Order_Response_Contains_Complete_Field()
    {
        using var c = ApiTestHelper.CreateClient();
        var id = (int)(ApiTestHelper.NewId() % 9_000_000 + 1);
        var create = await ApiTestHelper.CreateOrder(c, id);
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var body = await create.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.TryGetProperty("complete", out _), "The complete field is missing.");
        await ApiTestHelper.DeleteOrder(c, id);
    }

    [Test]
    public async Task Store_Inventory_Response_Is_Json()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("store/inventory");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        ApiTestHelper.Check(response.Content.Headers.ContentType?.MediaType?.Contains("json") == true, "Expected JSON content type.");
    }

    [Test]
    public async Task User_Create_And_Read()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("user");
        var create = await ApiTestHelper.CreateUser(c, username);
        ApiTestHelper.CheckStatus(create, HttpStatusCode.OK);
        var get = await c.GetAsync($"user/{Uri.EscapeDataString(username)}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        ApiTestHelper.Check(body.GetProperty("username").GetString() == username, "Username differs.");
        await ApiTestHelper.DeleteUser(c, username);
    }

    [Test]
    public async Task User_Create_And_Update()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("user");
        await ApiTestHelper.CreateUser(c, username);
        var update = await c.PutAsJsonAsync($"user/{Uri.EscapeDataString(username)}", new { id = ApiTestHelper.NewId(), username, firstName = "Changed", lastName = "User", email = $"{username}@example.com", password = "TestPass123", phone = "123456789", userStatus = 2 });
        ApiTestHelper.CheckStatus(update, HttpStatusCode.OK);
        var get = await c.GetAsync($"user/{Uri.EscapeDataString(username)}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.OK);
        await ApiTestHelper.DeleteUser(c, username);
    }

    [Test]
    public async Task User_Delete_Existing()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("user");
        await ApiTestHelper.CreateUser(c, username);
        var delete = await c.DeleteAsync($"user/{Uri.EscapeDataString(username)}");
        ApiTestHelper.CheckStatus(delete, HttpStatusCode.OK);
        var get = await c.GetAsync($"user/{Uri.EscapeDataString(username)}");
        ApiTestHelper.CheckStatus(get, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task User_Get_Nonexistent()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync($"user/{Uri.EscapeDataString(ApiTestHelper.NewName("missing"))}");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task User_Delete_Nonexistent()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.DeleteAsync($"user/{Uri.EscapeDataString(ApiTestHelper.NewName("missing"))}");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Test]
    public async Task User_Login_After_Creation()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("login");
        const string password = "TestPass123";
        await c.PostAsJsonAsync("user", new { id = ApiTestHelper.NewId(), username, firstName = "Login", lastName = "Test", email = $"{username}@example.com", password, phone = "123456789", userStatus = 1 });
        var login = await c.GetAsync($"user/login?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}");
        ApiTestHelper.CheckStatus(login, HttpStatusCode.OK);
        var text = await login.Content.ReadAsStringAsync();
        ApiTestHelper.Check(text.Contains("logged in", StringComparison.OrdinalIgnoreCase), "Login message was not returned.");
        await ApiTestHelper.DeleteUser(c, username);
    }

    [Test]
    public async Task User_Logout()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.GetAsync("user/logout");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
    }

    [Test]
    public async Task User_Create_With_Array()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("array");
        var payload = new[] { new { id = ApiTestHelper.NewId(), username, firstName = "Array", lastName = "User", email = $"{username}@example.com", password = "TestPass123", phone = "123456789", userStatus = 1 } };
        var response = await c.PostAsJsonAsync("user/createWithArray", payload);
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        await ApiTestHelper.DeleteUser(c, username);
    }

    [Test]
    public async Task User_Create_With_List()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("list");
        var payload = new[] { new { id = ApiTestHelper.NewId(), username, firstName = "List", lastName = "User", email = $"{username}@example.com", password = "TestPass123", phone = "123456789", userStatus = 1 } };
        var response = await c.PostAsJsonAsync("user/createWithList", payload);
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        await ApiTestHelper.DeleteUser(c, username);
    }

    [Test]
    public async Task User_Create_Empty_Array()
    {
        using var c = ApiTestHelper.CreateClient();
        var response = await c.PostAsJsonAsync("user/createWithArray", Array.Empty<object>());
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task User_Login_Unknown_User()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("unknown");
        var response = await c.GetAsync($"user/login?username={Uri.EscapeDataString(username)}&password=wrong");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK, HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        ApiTestHelper.Check(!string.IsNullOrWhiteSpace(body), "Response body should not be empty.");
    }

    [Test]
    public async Task User_Update_Unknown_User()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("missing");
        var response = await c.PutAsJsonAsync($"user/{Uri.EscapeDataString(username)}", new { id = ApiTestHelper.NewId(), username, firstName = "No", lastName = "User", email = "none@example.com", password = "TestPass123", phone = "000", userStatus = 0 });
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Test]
    public async Task User_Create_Response_Is_Not_Empty()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("created");
        var response = await ApiTestHelper.CreateUser(c, username);
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        ApiTestHelper.Check(!string.IsNullOrWhiteSpace(body), "Create response should not be empty.");
        await ApiTestHelper.DeleteUser(c, username);
    }

    [Test]
    public async Task User_Get_Response_Is_Json()
    {
        using var c = ApiTestHelper.CreateClient();
        var username = ApiTestHelper.NewName("jsonuser");
        await ApiTestHelper.CreateUser(c, username);
        var response = await c.GetAsync($"user/{Uri.EscapeDataString(username)}");
        ApiTestHelper.CheckStatus(response, HttpStatusCode.OK);
        ApiTestHelper.Check(response.Content.Headers.ContentType?.MediaType?.Contains("json") == true, "Expected JSON content type.");
        await ApiTestHelper.DeleteUser(c, username);
    }

}
