using System.Reflection.PortableExecutable;

namespace StockManagement.Kernel;


internal class MachineManager
{
	private List<Machine> _machines = new List<Machine>();


	internal void Init ()
	{
		_machines.Clear();
	}
}
