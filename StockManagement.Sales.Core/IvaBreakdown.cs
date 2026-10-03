using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core;


/// <summary>
/// Splits invoice lines into Paraguay's three IVA buckets (10%/5%/exempt), same grouping as <see cref="InvoiceCalculator.CalculateTax"/>
/// but kept per rate instead of summed, for the Marangatu export (#123).
/// </summary>
internal readonly record struct IvaBreakdown(decimal Taxed10, decimal Vat10, decimal Taxed5, decimal Vat5, decimal Exempt)
{
	public static IvaBreakdown Calculate(IEnumerable<ShoppingCartItem> items, int currencyDecimalDigits)
	{
		decimal taxed10 = 0, vat10 = 0, taxed5 = 0, vat5 = 0, exempt = 0;
		foreach (var group in (items ?? []).GroupBy(item => item.StockItem.VatRatePercent))
		{
			var groupTotal = group.Sum(item => item.Amount * Round(item.StockItem.Price, currencyDecimalDigits));
			var vatShare = Round(groupTotal * group.Key / (100 + group.Key), currencyDecimalDigits);
			var taxedBase = groupTotal - vatShare;

			if (group.Key == 10) { taxed10 += taxedBase; vat10 += vatShare; }
			else if (group.Key == 5) { taxed5 += taxedBase; vat5 += vatShare; }
			else exempt += groupTotal;
		}

		return new IvaBreakdown(taxed10, vat10, taxed5, vat5, exempt);
	}

	private static decimal Round(decimal value, int digits)
	{
		return Math.Round(value, digits, MidpointRounding.AwayFromZero);
	}
}
