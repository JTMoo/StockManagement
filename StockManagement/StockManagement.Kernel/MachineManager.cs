using StockManagement.Kernel.Model;
using System.Diagnostics;

namespace StockManagement.Kernel;


internal class MachineManager
{
	private List<Machine> _machines = new List<Machine>();


	internal void Init ()
	{
		_machines.Clear();
	}

	internal void Register(Machine machine)
	{
		if (_machines.Contains(machine)) return;

		_machines.Add(machine);
		Trace.WriteLine("Machine added.");
	}
}
