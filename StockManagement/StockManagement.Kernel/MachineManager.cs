using System.Reflection.PortableExecutable;

namespace StockManagement.Kernel;


internal class MachineManager
{
	private List<Machine> machines = new List<Machine>();


	internal void Init ()
	{
		this.machines.Clear();
	}
}
