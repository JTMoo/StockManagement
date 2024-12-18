using System.Linq.Expressions;
using MongoDB.Driver;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IDatabase
{
	public Task<IEnumerable<T>> GetAll<T>();
	public Task<T> GetOneAsync<T>(Expression<Func<T, bool>> filter);
	public Task Add<T>(BaseDocument item);
	public Task<DeleteResult> Delete<T>(BaseDocument item);
	public Task<ReplaceOneResult> Update<T>(BaseDocument item);
	public IMongoCollection<T> ConnectToMongo<T>(in string collectionName);
	public IMongoCollection<T> ConnectToMongo<T>();
}
