using System.Text.Json.Serialization;
using FastEndpoints;
using StockManagement.Customers.Core;
using StockManagement.Kernel;
using StockManagement.Kernel.Database;
using StockManagement.Sales.Core;

var builder = WebApplication.CreateBuilder(args);

var database = new DatabaseManager(builder.Configuration.GetMongoDatabase());

builder.Services
	.AddFastEndpoints()
	.AddKernel(database)
	.AddSalesCore()
	.AddCustomersCore();

var app = builder.Build();

await database.CreateUniqueIndexesAsync();

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
