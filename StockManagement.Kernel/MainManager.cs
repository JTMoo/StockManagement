using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Database;
using System.Diagnostics;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Diagnostics;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Kernel;


/// ********************************************************************************************************************************
/// <summary>
/// Singleton - Main - Central hub for logic inside the Kernel
/// </summary>
/// ********************************************************************************************************************************
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
    }

    ~MainManager()
    {
        this.Dispose();
    }

    public DatabaseManager DatabaseManager { get; } = new();
	public Settings Settings { get; internal set; }


    public static async Task<IDatabase> Initialize()
    {
        if (_isInitialized) return Instance.DatabaseManager;

        await Instance.Init();
        return Instance.DatabaseManager;
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

	private async Task Init()
	{
		this.Settings = await DatabaseManager.GetOneAsync<Settings>(_ => true).ContinueWith(task => task.Result ?? new());
		this.CreateCollectionIndeces();

		this._commandManager.Init();
		_isInitialized = true;
	}

	private void CreateCollectionIndeces()
	{
		if (!this.Settings.StockItemsIndexCreated)
		{
			var collection = this.DatabaseManager.ConnectToMongo<StockItem>();
			collection.Indexes.CreateMany(UniquePropertyHelper.GetStockItemUniqueProperties());
            this.Settings.StockItemsIndexCreated = true;
		}
	}
}