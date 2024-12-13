using MongoDB.Driver;

namespace StockManagement.Kernel.Database;


public interface IDatabase
{
	public Task<List<T>> GetAll<T>();
	public T GetFirst<T>();
	public Task Add<T>(BaseDocument item);
	public Task Delete<T>(BaseDocument item);
	public Task Update<T>(BaseDocument item);
	public IMongoCollection<T> ConnectToMongo<T>(in string collectionName);
}
