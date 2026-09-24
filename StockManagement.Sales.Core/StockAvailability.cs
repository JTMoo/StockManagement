namespace StockManagement.Sales.Core;


internal static class StockAvailability
{
	/// <summary>
	/// Returns every article whose requested units exceed the units in stock
	/// </summary>
	/// <remarks>Requests for the same <see cref="StockRequest.Code"/> are added up first.</remarks>
	/// <returns>Empty when the whole sale can be served</returns>
	public static IReadOnlyList<StockRequest> FindShortages(IEnumerable<StockRequest> requests)
	{
		if (requests == null) return [];

		return requests
			.GroupBy(request => request.Code)
			.Select(group => group.First() with { Requested = group.Sum(request => request.Requested) })
			.Where(request => request.Requested > request.InStock)
			.ToList();
	}
}
