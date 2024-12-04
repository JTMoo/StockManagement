using System.Diagnostics;

namespace StockManagement.Kernel.Diagnostics;


public class DateTimeTextWriterTraceListener(FileStream stream) : TextWriterTraceListener(stream)
{
	public override void WriteLine(string? message)
	{
		base.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.ffff ") + message);
	}
}
