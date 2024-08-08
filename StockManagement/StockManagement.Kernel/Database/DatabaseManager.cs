using MongoDB.Driver;

namespace StockManagement.Kernel.Database;


public class DatabaseManager
{
    private const string ConnectionString = "mongodb://127.0.0.1:27017";
    private const string DatabaseName = "LaCosecha_StockManagement";


    public DatabaseManager ()
    {
        this.MachineDB = new MachineDataAccess();
        this.SparePartDB = new SparePartDataAccess();
        this.TireDB = new TireDataAccess();
        this.TransactionDB = new TransactionDataAccess();
        this.SettingsDB = new SettingsDataAccess();
    }

    internal MachineDataAccess MachineDB { get; }
    internal SparePartDataAccess SparePartDB { get; }
    internal TireDataAccess TireDB { get; }
    internal TransactionDataAccess TransactionDB { get; }
    internal SettingsDataAccess SettingsDB { get; }


    internal static IMongoCollection<T> ConnectToMongo<T>(in string collectionName)
    {
        var client = new MongoClient(ConnectionString);
        var db = client.GetDatabase(DatabaseName);
        return db.GetCollection<T>(collectionName);
    }
}