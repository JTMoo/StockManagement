namespace StockManagement.Kernel.Database.Interfaces;


/// <summary>
/// Cross-domain search (ADR-0026): Postgres full-text + trigram, one query per domain
/// </summary>
public interface ISearchServiceProvider
{
	/// <param name="query">Raw user input; never interpreted as a client-constructed tsquery</param>
	/// <param name="includeInactive">Widens past the active-only default (e.g. cancelled invoices)</param>
	/// <param name="domains">Which <see cref="SearchDomains"/> to query; caller filters by permission</param>
	Task<IReadOnlyList<SearchGroup>> SearchAsync(string query, bool includeInactive, IReadOnlySet<string> domains, CancellationToken cancellationToken = default);
}


/// <summary>One domain's result, ranked and capped; <see cref="TotalCount"/> is the unranked match count</summary>
public sealed record SearchGroup(string Domain, IReadOnlyList<SearchHit> Items, int TotalCount);


public sealed record SearchHit(string Id, string Title, string Subtitle);


public static class SearchDomains
{
	public const string StockItems = "StockItems";
	public const string Customers = "Customers";
	public const string Invoices = "Invoices";
	public const string Suppliers = "Suppliers";
}
