using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StockManagement.Kernel.Tests")]
namespace StockManagement.Kernel.Commands
{
    internal class CommandQueue
    {
        List<ICommand> queque = new List<ICommand>();

        public bool Add (ICommand command)
        {
            if (command == null) return false;

            this.queque.Add(command);
            return true;
        }

        public ICommand? Pop ()
        {
            if (this.queque.Count == 0) return null;

            return this.queque[0];
        }
    }
}