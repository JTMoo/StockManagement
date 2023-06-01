using StockManagement.Kernel.Model;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class TireManager : NotificationBase
{
	private List<Tire> _tires = new List<Tire>();

	public List<Tire> Tires
	{
		get { return _tires; }
		private set { this.SetField(ref this._tires, value); }
	}

	internal void Init()
	{
		this._tires.Clear();
	}

	internal void Register(Tire tire)
	{
		if (tire == null) return;

		_tires.Add(tire);
		MainManager.Instance.DatabaseManager.TireCollection.InsertOneAsync(tire);
		Trace.WriteLine("Tire added.");
	}
}