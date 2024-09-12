using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class SparePartManager : NotificationBase
{
	private ObservableCollection<SparePart> _spareParts = new();

	public ObservableCollection<SparePart> SpareParts
	{
		get { return _spareParts; }
		private set { this.SetField(ref this._spareParts, value); }
	}

	internal async void Init()
	{
		this.SpareParts.Clear();
		var spareParts = await Database.SparePartDataAccess.GetAll();
		spareParts.ForEach(sparePart => this.SpareParts.Add(sparePart));
	}

	internal void Register(SparePart sparePart)
	{
		if (sparePart == null) return;

		_spareParts.Add(sparePart);
		Database.SparePartDataAccess.Add(sparePart);
		Trace.WriteLine("Spare Part added.");
	}

	internal void Update(SparePart sparePart, Action callback)
	{
		if (!_spareParts.Contains(sparePart)) return;

		Database.SparePartDataAccess.Update(sparePart).ContinueWith(_ => callback.Invoke());
	}
}