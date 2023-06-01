using StockManagement.Kernel.Model;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class MachineManager : NotificationBase
{
	private List<Machine> _machines = new List<Machine>();

	public List<Machine> Machines
	{
		get { return _machines; }
		private set { this.SetField(ref this._machines, value); }
	}

	internal void Init ()
	{
		_machines.Clear();
	}

	internal void Register(Machine machine)
	{
		if (_machines.Contains(machine)) return;

		_machines.Add(machine);
		MainManager.Instance.DatabaseManager.MachineCollection.InsertOneAsync(machine);
		Trace.WriteLine("Machine added.");
	}
}