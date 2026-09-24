using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StockManagement.Infrastructure;
using StockManagement.Infrastructure.Database;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;
using Testcontainers.PostgreSql;

namespace StockManagement.Api.Tests.Infrastructure;


[TestClass]
public sealed class AppDbContextTests
{
	private static readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16").Build();

	private ServiceProvider _services;
	private List<string> _log;


	[ClassInitialize]
	public static Task StartAsync(TestContext _)
	{
		return _container.StartAsync();
	}

	[ClassCleanup]
	public static ValueTask StopAsync()
	{
		return _container.DisposeAsync();
	}

	[TestInitialize]
	public async Task InitializeAsync()
	{
		var connectionString = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = $"test_{Guid.NewGuid():N}" }.ConnectionString;
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection([new("ConnectionStrings:Postgres", connectionString)])
			.Build();

		_log = [];
		_services = new ServiceCollection()
			.AddInfrastructure(configuration)
			.AddSingleton(_log)
			.AddScoped<IEntityChangedHandler<StockItem>, SetHandler>()
			.BuildServiceProvider();

		await using var scope = _services.CreateAsyncScope();
		await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
	}

	[TestCleanup]
	public ValueTask CleanupAsync()
	{
		return _services.DisposeAsync();
	}


	[TestMethod]
	public async Task SaveChangesAsync_HandlerAddsItem_ChainHandledAndBothSaved()
	{
		// Act
		await this.SaveAsync(db => db.StockItems.Add(new StockItem("Screw", code: "A1")));

		// Assert
		CollectionAssert.AreEqual(new[] { "Added A1", "Added A1-SET" }, _log);
		CollectionAssert.AreEquivalent(new[] { "A1", "A1-SET" }, await this.GetCodesAsync());
	}

	[TestMethod]
	public async Task SaveChangesAsync_ItemChanged_HandlerCalledWithModified()
	{
		// Arrange
		await this.SaveAsync(db => db.StockItems.Add(new StockItem("Nut", code: "B2")));
		_log.Clear();

		// Act
		await this.SaveAsync(async db => (await db.StockItems.SingleAsync()).Amount = 7);

		// Assert
		CollectionAssert.AreEqual(new[] { "Modified B2" }, _log);
	}

	[TestMethod]
	public async Task SaveChangesAsync_HandlerAddsDuplicateCode_NothingSaved()
	{
		// Arrange
		await this.SaveAsync(db => db.StockItems.Add(new StockItem("Set", code: "A1-SET")));

		// Act
		var save = () => this.SaveAsync(db => db.StockItems.Add(new StockItem("Screw", code: "A1")));

		// Assert
		await Assert.ThrowsExceptionAsync<DbUpdateException>(save);
		CollectionAssert.AreEqual(new[] { "A1-SET" }, await this.GetCodesAsync());
	}


	private Task SaveAsync(Action<AppDbContext> change)
	{
		return this.SaveAsync(db =>
		{
			change(db);
			return Task.CompletedTask;
		});
	}

	private async Task SaveAsync(Func<AppDbContext, Task> change)
	{
		await using var scope = _services.CreateAsyncScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		await change(db);
		await db.SaveChangesAsync();
	}

	private async Task<List<string>> GetCodesAsync()
	{
		await using var scope = _services.CreateAsyncScope();
		return await scope.ServiceProvider.GetRequiredService<AppDbContext>().StockItems.Select(item => item.Code).ToListAsync();
	}


	/// <summary>
	/// Logs each change; a new "A1" adds its set "A1-SET"
	/// </summary>
	private sealed class SetHandler(AppDbContext db, List<string> log) : IEntityChangedHandler<StockItem>
	{
		public Task OnChangedAsync(StockItem item, EntityChangeType changeType, CancellationToken cancellationToken)
		{
			log.Add($"{changeType} {item.Code}");
			if (changeType == EntityChangeType.Added && item.Code == "A1") db.StockItems.Add(new StockItem("Set", code: "A1-SET"));
			return Task.CompletedTask;
		}
	}
}
