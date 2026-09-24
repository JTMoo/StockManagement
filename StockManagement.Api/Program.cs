using System.Text.Json.Serialization;
using FastEndpoints;
using MongoDB.Driver;
using StockManagement.Customers.Core;
using StockManagement.Kernel;
using StockManagement.Kernel.Database;
using StockManagement.Sales.Core;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Mongo") ?? throw new InvalidOperationException("ConnectionStrings:Mongo is missing.");
var databaseName = builder.Configuration["Mongo:DatabaseName"] ?? throw new InvalidOperationException("Mongo:DatabaseName is missing.");
var database = new DatabaseManager(new MongoClient(connectionString).GetDatabase(databaseName));

builder.Services
	.AddFastEndpoints()
	.AddKernel(database)
	.AddSalesCore()
	.AddCustomersCore();

var app = builder.Build();

await database.CreateUniqueIndexesAsync();

// React build (StockManagement.Web) lands in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseFastEndpoints(config =>
{
	config.Endpoints.RoutePrefix = "api";
	config.Errors.UseProblemDetails();
	config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});

await app.RunAsync();


/// <summary>
/// Entry point, public for <c>WebApplicationFactory</c>
/// </summary>
public partial class Program;
