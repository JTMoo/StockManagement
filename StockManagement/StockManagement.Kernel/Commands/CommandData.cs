using System.Diagnostics;

namespace StockManagement.Kernel.Commands;


public class CommandData
{
    public Action<bool> Callback { get; set; }

    public Enum Type { get; set; }

    public void InvokeCallback(bool success)
    {
        try
        {
            Callback?.Invoke(success);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }
}