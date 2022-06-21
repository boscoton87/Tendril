using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tendril.Models;

namespace Tendril.Services.Interfaces {
	/// <summary>
	/// Interface for DataCollection CRUD Operations
	/// </summary>
	public interface IDataCollection {
		/// <summary>
		/// Get an instance of the connection
		/// </summary>
		/// <returns>The connection instance</returns>
		public IDisposable GetConnection();

		/// <summary>
		/// Apply the pending changes to the datasource
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <returns></returns>
		public Task SaveChangesAsync( object connection );

		/// <summary>
		/// Execute a raw query against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="query">The query to execute</param>
		/// <param name="parameters">params style array of parameters to pass into query</param>
		/// <returns>Collection of results from the query</returns>
		public Task<IEnumerable<TEntity>> ExecuteRawQuery<TEntity>( object connection, string query, object[] parameters ) where TEntity : class;

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public TEntity Add<TEntity>( object connection, TEntity entity ) where TEntity : class;

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entities">The entities to insert</param>
		public void AddRange<TEntity>( object connection, IEnumerable<TEntity> entities ) where TEntity : class;

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public TEntity Update<TEntity>( object connection, TEntity entity ) where TEntity : class;

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entity">The entity to delete</param>
		public void Delete<TEntity>( object connection, TEntity entity ) where TEntity : class;

		/// <summary>
		/// Execute a query with custom conditions against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public Task<ValidationDataResult<IEnumerable<TEntity>>> FindByFilter<TEntity>(
			object connection,
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class;
	}
}
