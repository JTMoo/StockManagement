using MongoDB.Driver;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class SparePartManager : NotificationBase
{
	private ObservableCollection<SparePart> _spareParts = new ObservableCollection<SparePart>();

	public ObservableCollection<SparePart> SpareParts
	{
		get { return _spareParts; }
		private set { this.SetField(ref this._spareParts, value); }
	}

	internal async void Init()
	{
		this.SpareParts.Clear();
		var spareParts = await MainManager.Instance.DatabaseManager.SparePartDB.GetAll();
		spareParts.ForEach(sparePart => this.SpareParts.Add(sparePart));
	}

	internal void Register(SparePart sparePart)
	{
		if (sparePart == null) return;

		_spareParts.Add(sparePart);
		MainManager.Instance.DatabaseManager.SparePartDB.Add(sparePart);
		Trace.WriteLine("Spare Part added.");
	}
}