namespace StockManagement.Kernel.Model;


/// <summary>
/// Single pre-assigned CDC document-number range DNIT granted for SIFEN contingency use (#149) - issuing locally
/// with <c>EmissionType.Contingencia</c> while fully offline. <see cref="NextNumber"/> is the reservation cursor;
/// replaced wholesale (same singleton-row pattern as <see cref="AppSettings"/>) when a new range is obtained.
/// </summary>
public class ContingencyCdcRange : Database.BaseDocument
{
	private long rangeStart;
	private long rangeEnd;
	private long nextNumber;
	private bool isActive;


	public long RangeStart
	{
		get { return this.rangeStart; }
		set { this.SetField(ref this.rangeStart, value); }
	}

	public long RangeEnd
	{
		get { return this.rangeEnd; }
		set { this.SetField(ref this.rangeEnd, value); }
	}

	/// <summary>
	/// Next document number to reserve; exhausted once past <see cref="RangeEnd"/>
	/// </summary>
	public long NextNumber
	{
		get { return this.nextNumber; }
		set { this.SetField(ref this.nextNumber, value); }
	}

	/// <summary>
	/// Operator-declared contingency event in progress; invoices are issued from this range only while set
	/// </summary>
	public bool IsActive
	{
		get { return this.isActive; }
		set { this.SetField(ref this.isActive, value); }
	}
}
