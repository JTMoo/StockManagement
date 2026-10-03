using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Api.Features.Sifen;


public sealed record ContingencyStatusResponse(bool RangeConfigured, bool IsActive, long RangeStart, long RangeEnd, long NextNumber, long Remaining)
{
	public static ContingencyStatusResponse From(ContingencyStatus status)
	{
		return new(status.RangeConfigured, status.IsActive, status.RangeStart, status.RangeEnd, status.NextNumber, status.Remaining);
	}
}
