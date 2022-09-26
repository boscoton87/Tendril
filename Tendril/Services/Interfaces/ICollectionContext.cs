using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tendril.Models;

namespace Tendril.Services.Interfaces {
	/// <summary>
	/// Interface for DataCollection CRUD Operations
	/// </summary>
	internal interface ICollectionContext {

		/// <summary>
		/// Execute a raw query against the underlying datasource
		/// </summary>
		/// <param name="query">The query to execute</param>
		/// <param name="parameters">params style array of parameters to pass into query</param>
		/// <returns>Collection of results from the query</returns>
		public Task<IEnumerable<TEntity>> ExecuteRawQuery<TEntity>( string query, object[] parameters ) where TEntity : class;

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public Task<TEntity> Add<TEntity>( TEntity entity ) where TEntity : class;

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <param name="entities">The entities to insert</param>
		public Task AddRange<TEntity>( IEnumerable<TEntity> entities ) where TEntity : class;

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public Task<TEntity> Update<TEntity>( TEntity entity ) where TEntity : class;

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <param name="entity">The entity to delete</param>
		public Task Delete<TEntity>( TEntity entity ) where TEntity : class;

		/// <summary>
		/// Execute a query with custom conditions against the underlying datasource
		/// </summary>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public Task<ValidationDataResult<IEnumerable<TEntity>>> FindByFilter<TEntity>(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class;

		/// <summary>
		/// Execute a count query with custom conditions against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public Task<ValidationDataResult<long>> CountByFilter<TEntity>(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class;
	}
}
