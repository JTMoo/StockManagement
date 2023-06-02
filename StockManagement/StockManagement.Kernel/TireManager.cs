using MongoDB.Driver;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class TireManager : NotificationBase
{
	private ObservableCollection<Tire> _tires = new ObservableCollection<Tire>();

	public ObservableCollection<Tire> Tires
	{
		get { return _tires; }
		private set { this.SetField(ref this._tires, value); }
	}

	internal async void Init()
	{
		this.Tires.Clear();
		var tires = await MainManager.Instance.DatabaseManager.TireDB.GetAll();
		tires.ForEach(tire => this.Tires.Add(tire));
	}

	internal void Register(Tire tire)
	{
		if (tire == null) return;

		_tires.Add(tire);
		MainManager.Instance.DatabaseManager.TireDB.Add(tire);
		Trace.WriteLine("Tire added.");
	}
}