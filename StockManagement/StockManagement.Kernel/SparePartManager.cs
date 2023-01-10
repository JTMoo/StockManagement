using StockManagement.Kernel.Model;
using System.Diagnostics;

namespace StockManagement.Kernel;


internal class SparePartManager
{
	private List<SparePart> _spareParts = new List<SparePart>();


	internal void Init()
	{
		this._spareParts.Clear();
	}

	internal void Register(SparePart sparePart)
	{
		if (sparePart == null) return;

		_spareParts.Add(sparePart);
		Trace.WriteLine("Spare Part added.");
	}
}
