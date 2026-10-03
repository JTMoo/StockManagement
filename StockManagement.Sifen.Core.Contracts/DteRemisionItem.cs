namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// One line (<c>gCamItem</c>) in a Nota de Remisión DE - goods moved, no price or VAT (#162).
/// </summary>
public sealed record DteRemisionItem(
	string Code,
	string Description,
	decimal Quantity);
