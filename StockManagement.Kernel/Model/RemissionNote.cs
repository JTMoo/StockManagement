using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


/// <summary>
/// SIFEN Nota de Remisión Electrónica (#162) - the goods-movement document DNIT requires for physically moving
/// stock, independent of whether the movement is also an invoiced sale.
/// </summary>
public class RemissionNote : BaseDocument
{
	private string number = "";
	private DateTime date;
	private RemissionReason reason;
	private string destinationAddress = "";
	private string cdc = "";
	private TransmissionStatus transmissionStatus = TransmissionStatus.Pending;


	public RemissionNote()
	{
	}


	/// <summary>
	/// DNIT composite number (establishment-pointOfSale-sequence), own sequence scoped separately from
	/// <see cref="Invoice.Number"/>; see <see cref="Util.InvoiceNumber"/>.
	/// </summary>
	public string Number
	{
		get { return this.number; }
		set { this.SetField(ref this.number, value); }
	}

	public DateTime Date
	{
		get { return this.date; }
		set { this.SetField(ref this.date, value); }
	}

	/// <summary>
	/// SIFEN <c>dMotTras</c> - why the goods are moving
	/// </summary>
	public RemissionReason Reason
	{
		get { return this.reason; }
		set { this.SetField(ref this.reason, value); }
	}

	/// <summary>
	/// Where the goods are going (<c>dDirDest</c>); free text, not tied to <see cref="Customer.Address"/>
	/// </summary>
	public string DestinationAddress
	{
		get { return this.destinationAddress; }
		set { this.SetField(ref this.destinationAddress, value); }
	}

	/// <summary>
	/// Receiving party - the same <see cref="Model.Customer"/> entity used for sales (ADR-0031 receptor shape)
	/// </summary>
	public Customer Customer { get; set; } = null!;

	public List<RemissionNoteItem> Items { get; set; } = [];

	/// <summary>
	/// 44-digit SIFEN control code; empty until the first transmission attempt builds the DE
	/// </summary>
	public string Cdc
	{
		get { return this.cdc; }
		set { this.SetField(ref this.cdc, value); }
	}

	public TransmissionStatus TransmissionStatus
	{
		get { return this.transmissionStatus; }
		set { this.SetField(ref this.transmissionStatus, value); }
	}
}
