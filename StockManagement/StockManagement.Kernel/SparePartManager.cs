using StockManagement.Kernel.Model;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace StockManagement.Kernel;


public class SparePartManager : NotificationBase
{
	private List<SparePart> _spareParts = new List<SparePart>();

	public List<SparePart> SpareParts
	{
		get { return _spareParts; }
		private set { this.SetField(ref this._spareParts, value); }
	}

	internal void Init()
	{
		this._spareParts.Clear();
	}

	internal void Register(SparePart sparePart)
	{
		if (sparePart == null) return;

		_spareParts.Add(sparePart);
		MainManager.Instance.DatabaseManager.SparePartCollection.InsertOneAsync(sparePart);
		Trace.WriteLine("Spare Part added.");
	}
}
