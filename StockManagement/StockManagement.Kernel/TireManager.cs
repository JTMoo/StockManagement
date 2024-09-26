using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class TireManager : NotificationBase
{
	private readonly ObservableCollection<Tire> _editableTires = [];
	private readonly ReadOnlyObservableCollection<Tire> _tires;


	public TireManager()
	{
		this._tires = new(_editableTires);
	}

	public ReadOnlyObservableCollection<Tire> Tires
	{
		get { return _tires; }
	}

	internal async void Init()
	{
		this._editableTires.Clear();
		var tires = await Database.TireDataAccess.GetAll();
		tires.ForEach(tire => this._editableTires.Add(tire));
	}

	internal void Register(Tire tire)
	{
		if (tire == null) return;

		_editableTires.Add(tire);
		Database.TireDataAccess.Add(tire);
		Trace.WriteLine("Tire added.");
	}

	internal void Update(Tire tire, Action callback)
	{
		if (!_editableTires.Contains(tire)) return;

		Database.TireDataAccess.Update(tire).ContinueWith(_ => callback.Invoke());
	}
}