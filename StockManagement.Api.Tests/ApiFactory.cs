using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StockManagement.Api.Features.Auth;

namespace StockManagement.Api.Tests;


/// <summary>
/// API host on its own empty database in <see cref="PostgresContainer"/>
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
	/// <summary>
	/// Credentials of the admin user the <c>AddUsers</c> migration seeds
	/// </summary>
	public const string SeededAdminUsername = "admin";
	public const string SeededAdminPassword = "ChangeMe123!";

	public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

	private IServiceScope _scope;

	/// <summary>
	/// Scoped service provider; the Kernel service providers are Scoped (EF's AppDbContext), so <c>Services</c> (the root provider) can't resolve them directly
	/// </summary>
	public IServiceProvider ScopedServices => (_scope ??= this.Services.CreateScope()).ServiceProvider;


	/// <summary>
	/// A client logged in as the seeded admin user, with the JWT set as a bearer token
	/// </summary>
	public async Task<HttpClient> CreateAuthenticatedClientAsync()
	{
		var client = this.CreateClient();
		var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(SeededAdminUsername, SeededAdminPassword));
		var body = await response.Content.ReadAsAsync<LoginResponse>();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
		return client;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		var connectionString = new NpgsqlConnectionStringBuilder(PostgresContainer.ConnectionString) { Database = $"test_{Guid.NewGuid():N}" }.ConnectionString;
		builder.UseSetting("ConnectionStrings:Postgres", connectionString);
	}

	protected override void Dispose(bool disposing)
	{
		_scope?.Dispose();
		base.Dispose(disposing);
	}
}


public static class HttpContentExtensions
{
	public static Task<T> ReadAsAsync<T>(this HttpContent content)
	{
		return content.ReadFromJsonAsync<T>(ApiFactory.JsonOptions);
	}
}
