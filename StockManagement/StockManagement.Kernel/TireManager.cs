using StockManagement.Kernel.Model;
using System.Diagnostics;

namespace StockManagement.Kernel;

internal class TireManager
{
	private List<Tire> _tires = new List<Tire>();


	internal void Init()
	{
		this._tires.Clear();
	}

	internal void Register(Tire tire)
	{
		if (tire == null) return;

		_tires.Add(tire);
		Trace.WriteLine("Tire added.");
	}
}
