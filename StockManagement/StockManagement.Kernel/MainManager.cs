using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Database;
using System.Diagnostics;

namespace StockManagement.Kernel;


public class MainManager : NotificationBase, IDisposable
{
    public static readonly MainManager Instance = new MainManager();

	private CommandManager _commandManager;
    private DatabaseManager _databaseManager;
    private MachineManager _machineManager;
    private TireManager _tireManager;
    private SparePartManager _sparePartManager;


    public MainManager() 
    {
        _commandManager = new CommandManager();
		_databaseManager = new DatabaseManager();
        _machineManager = new MachineManager();
        _tireManager = new TireManager();
		_sparePartManager = new SparePartManager();
    }

    ~MainManager()
    {
        this.Dispose();
    }

    public DatabaseManager DatabaseManager => _databaseManager;
    public MachineManager MachineManager => _machineManager;
	public SparePartManager SparePartManager => _sparePartManager;
    public TireManager TireManager => _tireManager;

    public void Init()
    {
        _commandManager.Init();
        _machineManager.Init();
        _tireManager.Init();
        _sparePartManager.Init();
    }

    public void Dispose()
    {
        _commandManager.Dispose();
    }

    public async void StartObservedTask(Action action)
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

    public async void StartObservedTask(Action action, CancellationToken token)
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
}