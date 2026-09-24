using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StockManagement.Api.Tests;


/// <summary>
/// API host on its own empty database in <see cref="MongoContainer"/>
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
	public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };


	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseSetting("ConnectionStrings:Mongo", MongoContainer.ConnectionString);
		builder.UseSetting("Mongo:DatabaseName", $"test_{Guid.NewGuid():N}");
	}
}


public static class HttpContentExtensions
{
	public static Task<T> ReadAsAsync<T>(this HttpContent content)
	{
		return content.ReadFromJsonAsync<T>(ApiFactory.JsonOptions);
	}
}
