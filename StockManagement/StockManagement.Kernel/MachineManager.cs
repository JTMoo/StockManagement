using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class MachineManager : NotificationBase
{
	private readonly ObservableCollection<Machine> _editableMachines = [];
	private readonly ReadOnlyObservableCollection<Machine> _machines;


	public MachineManager()
	{
		this._machines = new(this._editableMachines);
	}

	public ReadOnlyObservableCollection<Machine> Machines
	{
		get { return _machines; }
	}

	internal async void Init ()
	{
		this._editableMachines.Clear();
		var machines = await Database.MachineDataAccess.GetAll();
		machines.ForEach(this._editableMachines.Add);
	}

	internal void Register(Machine machine)
	{
		if (_editableMachines.Contains(machine)) return;
		if (this.Machines.Any(existingMachine => existingMachine.Code == machine.Code))
		{
			Trace.WriteLine($"{Language.Resources.machine} with the same {Language.Resources.code} already exists: {machine}");
		}

		_editableMachines.Add(machine);
		Database.MachineDataAccess.Add(machine);
		Trace.WriteLine("Machine added.");
	}

	internal void Update(Machine machine, Action callback)
	{
		if (!_editableMachines.Contains(machine)) return;

		Database.MachineDataAccess.Update(machine).ContinueWith(_ => callback.Invoke());
	}
}