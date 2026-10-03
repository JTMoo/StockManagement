using StockManagement.Kernel.Model.Types;

namespace StockManagement.Sifen.Core.Contracts;


/// <summary>
/// Everything <see cref="IDteXmlBuilder"/> needs to build a Nota de Remisión Electrónica DE (#162).
/// </summary>
public sealed record DteRemisionData(
	string Cdc,
	DteEmisor Emisor,
	DteReceptor Receptor,
	DateTime IssueDate,
	long DocumentNumber,
	RemissionReason Reason,
	string DestinationAddress,
	IReadOnlyList<DteRemisionItem> Items);
