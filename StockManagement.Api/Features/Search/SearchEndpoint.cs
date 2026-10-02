using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Api.Features.Search;


public sealed record SearchRequest(string Q, bool? IncludeInactive);


public sealed record SearchHitResponse(string Id, string Title, string Subtitle);


public sealed record SearchGroupResponse(string Domain, IReadOnlyList<SearchHitResponse> Items, int TotalCount);


public sealed record SearchResponse(IReadOnlyList<SearchGroupResponse> Groups);


/// <summary>
/// Cross-domain search for the <c>CommandPalette</c> (ADR-0026); a group is omitted entirely when the caller
/// lacks that domain's read permission, rather than failing the whole request
/// </summary>
public class SearchEndpoint(ISearchServiceProvider searchServiceProvider) : Endpoint<SearchRequest, SearchResponse>
{
	private static readonly IReadOnlyDictionary<string, string> DomainPermissions = new Dictionary<string, string>
	{
		[Kernel.Database.Interfaces.SearchDomains.StockItems] = Permission.StockItemsRead,
		[Kernel.Database.Interfaces.SearchDomains.Customers] = Permission.CustomersRead,
		[Kernel.Database.Interfaces.SearchDomains.Invoices] = Permission.SalesRead,
		[Kernel.Database.Interfaces.SearchDomains.Suppliers] = Permission.SuppliersRead,
	};

	private readonly ISearchServiceProvider _searchServiceProvider = searchServiceProvider;


	public override void Configure()
	{
		this.Get("/search");
	}

	public override async Task<SearchResponse> ExecuteAsync(SearchRequest request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Q)) return new([]);

		var granted = this.User.Claims.Where(claim => claim.Type == "permissions").Select(claim => claim.Value).ToHashSet();
		var domains = DomainPermissions.Where(entry => granted.Contains(entry.Value)).Select(entry => entry.Key).ToHashSet();
		if (domains.Count == 0) return new([]);

		var groups = await _searchServiceProvider.SearchAsync(request.Q.Trim(), request.IncludeInactive ?? false, domains, cancellationToken);
		return new(groups.Select(group => new SearchGroupResponse(
			group.Domain,
			group.Items.Select(item => new SearchHitResponse(item.Id, item.Title, item.Subtitle)).ToList(),
			group.TotalCount)).ToList());
	}
}
