using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


/// ********************************************************************************************************************************
/// <summary>
/// Contains all Logic regarding <see cref="Tire"/>-Objects
/// </summary>
/// ********************************************************************************************************************************
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
		var tires = await MainManager.Instance.DatabaseManager.GetAll<Tire>().ContinueWith(task => task.Result.ToList());
		tires.ForEach(this._editableTires.Add);
	}

	internal void Register(Tire tire)
	{
		if (tire == null) return;
		if (this.Tires.Any(existingTire => existingTire.Code == tire.Code))
		{
			Trace.WriteLine($"{Language.Resources.tire} with the same {Language.Resources.code} already exists: {tire}");
			return;
		}

		_editableTires.Add(tire);
		MainManager.Instance.DatabaseManager.Add<Tire>(tire);
		Trace.WriteLine("Tire added.");
	}

	internal void Deregister(Tire tire)
	{
		if (tire == null) return;

		_editableTires.Remove(tire);
		MainManager.Instance.DatabaseManager.Delete<Tire>(tire);
		Trace.WriteLine("Tire deleted.");
	}

	internal void Update(Tire tire, Action callback)
	{
		if (!_editableTires.Contains(tire)) return;

		MainManager.Instance.DatabaseManager.Update<Tire>(tire).ContinueWith(_ => callback.Invoke());
	}
}