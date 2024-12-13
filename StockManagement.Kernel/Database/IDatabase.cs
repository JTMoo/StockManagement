using MongoDB.Driver;

namespace StockManagement.Kernel.Database;


public interface IDatabase
{
	public Task<IEnumerable<T>> GetAll<T>();
	public T GetFirst<T>();
	public Task Add<T>(BaseDocument item);
	public Task<DeleteResult> Delete<T>(BaseDocument item);
	public Task<ReplaceOneResult> Update<T>(BaseDocument item);
	public IMongoCollection<T> ConnectToMongo<T>(in string collectionName);
}
