using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class TireManager : NotificationBase
{
	private ObservableCollection<Tire> _tires = [];

	public ObservableCollection<Tire> Tires
	{
		get { return _tires; }
		private set { this.SetField(ref this._tires, value); }
	}

	internal async void Init()
	{
		this.Tires.Clear();
		var tires = await Database.TireDataAccess.GetAll();
		tires.ForEach(tire => this.Tires.Add(tire));
	}

	internal void Register(Tire tire)
	{
		if (tire == null) return;

		_tires.Add(tire);
		Database.TireDataAccess.Add(tire);
		Trace.WriteLine("Tire added.");
	}

	internal void Update(Tire tire, Action callback)
	{
		if (!_tires.Contains(tire)) return;

		Database.TireDataAccess.Update(tire).ContinueWith(_ => callback.Invoke());
	}
}