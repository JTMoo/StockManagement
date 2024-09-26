using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class MachineManager : NotificationBase
{
	private ObservableCollection<Machine> _machines = [];

	public ObservableCollection<Machine> Machines
	{
		get { return _machines; }
		private set { this.SetField(ref this._machines, value); }
	}

	internal async void Init ()
	{
		this.Machines.Clear();
		var machines = await Database.MachineDataAccess.GetAll();
		machines.ForEach(this.Machines.Add);
	}

	internal void Register(Machine machine)
	{
		if (_machines.Contains(machine)) return;

		_machines.Add(machine);
		Database.MachineDataAccess.Add(machine);
		Trace.WriteLine("Machine added.");
	}

	internal void Update(Machine machine, Action callback)
	{
		if (!_machines.Contains(machine)) return;

		Database.MachineDataAccess.Update(machine).ContinueWith(_ => callback.Invoke());
	}
}