using MongoDB.Driver;
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
		var spareParts = await MainManager.Instance.DatabaseManager.SparePartCollection.FindAsync(_ => true);
		await spareParts.ForEachAsync(sparePart => this.SpareParts.Add(sparePart));
	}

	internal void Register(SparePart sparePart)
	{
		if (sparePart == null) return;

		_spareParts.Add(sparePart);
		MainManager.Instance.DatabaseManager.SparePartCollection.InsertOneAsync(sparePart);
		Trace.WriteLine("Spare Part added.");
	}
}