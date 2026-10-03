namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// Current state of the SIFEN contingency CDC range (#149)
/// </summary>
public sealed record ContingencyStatus(bool RangeConfigured, bool IsActive, long RangeStart, long RangeEnd, long NextNumber)
{
	public long Remaining => this.RangeConfigured ? Math.Max(0, this.RangeEnd - this.NextNumber + 1) : 0;
}
