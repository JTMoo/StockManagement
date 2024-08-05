using MongoDB.Driver;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class MachineManager : NotificationBase
{
	private ObservableCollection<Machine> _machines = new ObservableCollection<Machine>();

	public ObservableCollection<Machine> Machines
	{
		get { return _machines; }
		private set { this.SetField(ref this._machines, value); }
	}

	internal async void Init ()
	{
		this.Machines.Clear();
		var machines = await MainManager.Instance.DatabaseManager.MachineDB.GetAll();
		machines.ForEach(machine => this.Machines.Add(machine));
	}

	internal void Register(Machine machine)
	{
		if (_machines.Contains(machine)) return;

		_machines.Add(machine);
		MainManager.Instance.DatabaseManager.MachineDB.Add(machine);
		Trace.WriteLine("Machine added.");
	}

	internal void Update(Machine machine, Action callback)
	{
		if (!_machines.Contains(machine)) return;

		MainManager.Instance.DatabaseManager.MachineDB.Update(machine).ContinueWith(_ => callback.Invoke());
	}
}