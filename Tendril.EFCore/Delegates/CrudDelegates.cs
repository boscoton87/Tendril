using Microsoft.EntityFrameworkCore;

namespace Tendril.EFCore.Delegates {

	/// <summary>
	/// Delegate defining how to get the required DbSet from a DbContext object
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <returns>The selected DbSet</returns>
	public delegate DbSet<TEntity> GetDbSet<TDataSource, TEntity>( TDataSource dbContext )
		where TDataSource : DbContext
		where TEntity : class;

	/// <summary>
	/// Delegate defining how to get an IQueryable representation of the data collection, 
	/// this is useful for specifying certain navigation properties to be included.
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <returns>The IQueryable representation to use for read operations</returns>
	public delegate IQueryable<TEntity> GetCollectionQueryable<TDataSource, TEntity>( TDataSource dbContext )
		where TDataSource : DbContext
		where TEntity : class;

	/// <summary>
	/// Delegate defining how to add an entity to the underlying DbSet
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <param name="entity">The entity to add</param>
	/// <returns>The added entity</returns>
	public delegate Task<TEntity> AddEntity<TDataSource, TEntity>( TDataSource dbContext, TEntity entity )
		where TDataSource : DbContext
		where TEntity : class;

	/// <summary>
	/// Delegate defining how to bulk add entities to the underlying DbSet
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <param name="entities">The entities to add</param>
	/// <returns></returns>
	public delegate Task AddEntities<TDataSource, TEntity>( TDataSource dbContext, IEnumerable<TEntity> entities )
		where TDataSource : DbContext
		where TEntity : class;

	/// <summary>
	/// Delegate defining how to delete an entity from the underlying DbSet
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <param name="entity">The entity to delete</param>
	/// <returns></returns>
	public delegate Task DeleteEntity<TDataSource, TEntity>( TDataSource dbContext, TEntity entity )
		where TDataSource : DbContext
		where TEntity : class;

	/// <summary>
	/// Delegate defining how to execute a raw query against the underlying database
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <param name="query">The query to execute</param>
	/// <param name="parameters">parameters to set for the query</param>
	/// <returns>The resulting entities from the query</returns>
	public delegate Task<IEnumerable<TEntity>> FindByRawQuery<TDataSource, TEntity>( TDataSource dbContext, string query, object[] parameters )
		where TDataSource : DbContext
		where TEntity : class;

	/// <summary>
	/// Delegate defining how to update an entity in the underlying DbSet
	/// </summary>
	/// <param name="dbContext">The relevant DbContext</param>
	/// <param name="entity">The entity containing updates</param>
	/// <returns>The updated entity</returns>
	public delegate Task<TEntity> UpdateEntity<TDataSource, TEntity>( TDataSource dbContext, TEntity entity )
		where TDataSource : DbContext
		where TEntity : class;
}
