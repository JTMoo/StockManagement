using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StockManagement.Kernel;


/// ********************************************************************************************************************************
/// <summary>
/// Manages all code regarding <see cref="Machine">-Objects
/// </summary>
/// ********************************************************************************************************************************
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
		var machines = await MainManager.Instance.DatabaseManager.GetAll<Machine>();
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
		MainManager.Instance.DatabaseManager.Add<Machine>(machine);
		Trace.WriteLine("Machine added.");
	}

	internal void Deregister(Machine machine)
	{
		if (machine == null) return;

		_editableMachines.Remove(machine);
		MainManager.Instance.DatabaseManager.Delete<Machine>(machine);
		Trace.WriteLine("Machine deleted.");
	}

	internal void Update(Machine machine, Action callback)
	{
		if (!_editableMachines.Contains(machine)) return;

		MainManager.Instance.DatabaseManager.Update<Machine>(machine).ContinueWith(_ => callback.Invoke());
	}
}