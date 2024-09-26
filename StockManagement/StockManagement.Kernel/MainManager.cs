using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Database;
using System.Diagnostics;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;


public class MainManager : NotificationBase, IDisposable
{
    internal static readonly MainManager Instance = new();

	private readonly CommandManager _commandManager = new();
	private static readonly bool _isInitialized;
	private static bool _disposed;

	public MainManager() 
    {
		this.Settings = SettingsDataAccess.Get() ?? new();
    }

    ~MainManager()
    {
        this.Dispose();
    }

    public DatabaseManager DatabaseManager { get; } = new();
	public MachineManager MachineManager { get; } = new();
	public SparePartManager SparePartManager { get; } = new();
	public TireManager TireManager { get; } = new();
	public Settings Settings { get; }


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
	}
}