using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Database;
using System.Diagnostics;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Diagnostics;

namespace StockManagement.Kernel;


public class MainManager : NotificationBase, IDisposable
{
    internal static readonly MainManager Instance = new();

	private readonly CommandManager _commandManager = new();
	private static bool _isInitialized = false;
	private static bool _disposed;

	private readonly string logFilePath = Path.Combine(AppContext.BaseDirectory, DateTime.Now.ToString("MM") + "_log.txt");


	private MainManager() 
    {
		Trace.Listeners.Add(new DateTimeTextWriterTraceListener(new FileStream(this.logFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)));
		this.Settings = DatabaseManager.GetFirst<Settings>() ?? new();
    }

    ~MainManager()
    {
        this.Dispose();
    }

    public DatabaseManager DatabaseManager { get; } = new();
	public MachineManager MachineManager { get; } = new();
	public SparePartManager SparePartManager { get; } = new();
	public TireManager TireManager { get; } = new();
	public Settings Settings { get; internal set; }


    public static void Initialize()
    {
        if (_isInitialized) return;

        Instance.Init();
    }


    public static void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
		{
			Instance.Dispose();
		}

        _disposed = true;
	}

	public void Dispose()
	{
		_commandManager.Dispose();
		GC.SuppressFinalize(this);
	}

	public static async void StartObservedTask(Action action)
    {
        try
        {
            await Task.Factory.StartNew(action);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    public static async void StartObservedTask(Action action, CancellationToken token)
    {
        try
        {
            await Task.Factory.StartNew(action, token);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    public bool PushCommand(ICommand command)
    {
        return _commandManager.Push(command);
	}

	private void Init()
	{
		this._commandManager.Init();
		this.MachineManager.Init();
		this.TireManager.Init();
		this.SparePartManager.Init();
        _isInitialized = true;
	}
}