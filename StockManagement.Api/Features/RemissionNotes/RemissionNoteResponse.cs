using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.RemissionNotes;


/// <summary>
/// Stored remission note (#162)
/// </summary>
public sealed record RemissionNoteResponse(string Number, DateTime Date, RemissionReason Reason, string DestinationAddress, int CustomerId, string CustomerName, string Cdc, TransmissionStatus TransmissionStatus, IReadOnlyList<RemissionNoteLineResponse> Lines)
{
	public static RemissionNoteResponse From(RemissionNote remissionNote)
	{
		var lines = (remissionNote.Items ?? []).Select(item => new RemissionNoteLineResponse(item.StockItem.Code, item.StockItem.Name, item.Amount)).ToList();
		var customerName = remissionNote.Customer is Customer customer ? string.Join(" ", customer.Name, customer.Lastname).Trim() : "";
		return new(remissionNote.Number, remissionNote.Date, remissionNote.Reason, remissionNote.DestinationAddress, remissionNote.Customer?.CustomerId ?? 0, customerName, remissionNote.Cdc, remissionNote.TransmissionStatus, lines);
	}
}


public sealed record RemissionNoteLineResponse(string Code, string Name, int Amount);
