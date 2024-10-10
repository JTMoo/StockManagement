using DocumentFormat.OpenXml.Drawing;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class SparePartManager : NotificationBase
{
	private readonly ObservableCollection<SparePart> _editableSpareParts = [];
	private readonly ReadOnlyObservableCollection<SparePart> _spareParts;


	public SparePartManager()
	{
		this._spareParts = new(_editableSpareParts);
	}

	public ReadOnlyObservableCollection<SparePart> SpareParts
	{
		get { return _spareParts; }
	}

	internal async void Init()
	{
		this._editableSpareParts.Clear();
		var spareParts = await Database.SparePartDataAccess.GetAll();
		spareParts.ForEach(this._editableSpareParts.Add);
	}

	internal void Register(SparePart sparePart)
	{
		if (sparePart == null) return;
		if (this.SpareParts.Any(existingSparePart => existingSparePart.Code == sparePart.Code))
		{
			Trace.WriteLine($"{Language.Resources.sparePart} with the same {Language.Resources.code} already exists: {sparePart}");
		}

		_editableSpareParts.Add(sparePart);
		Database.SparePartDataAccess.Add(sparePart);
		Trace.WriteLine("Spare Part added.");
	}

	internal void Update(SparePart sparePart, Action callback)
	{
		if (!_editableSpareParts.Contains(sparePart)) return;

		Database.SparePartDataAccess.Update(sparePart).ContinueWith(_ => callback.Invoke());
	}
}