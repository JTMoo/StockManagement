namespace StockManagement.Kernel.Database;


/// <summary>
/// Reacts to a saved change of a <typeparamref name="TEntity"/>
/// </summary>
/// <remarks>
/// Runs inside <c>SaveChangesAsync</c>, before the commit.
/// Changes made here are saved in the same transaction and raise their own handlers.
/// Throwing aborts the whole save.
/// </remarks>
public interface IEntityChangedHandler<in TEntity> where TEntity : class
{
	/// <summary>
	/// Called once per changed entity and change type
	/// </summary>
	Task OnChangedAsync(TEntity entity, EntityChangeType changeType, CancellationToken cancellationToken);
}
