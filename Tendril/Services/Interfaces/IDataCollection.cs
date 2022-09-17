﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Tendril.Models;

namespace Tendril.Services.Interfaces {
	/// <summary>
	/// Interface for DataCollection CRUD Operations
	/// </summary>
	/// <typeparam name="TEntity">The type of entity</typeparam>
	public interface IDataCollection<TEntity>
		where TEntity : class {

		/// <summary>
		/// Execute a raw query against the underlying datasource<br />
		/// <b>Note:</b>This operation should only be performed for performance critical code,<br />
		/// or if the required operation is not possible using the other CRUD methods, as this exposes implementation details<br />
		/// </summary>
		/// <param name="query">The query to execute</param>
		/// <param name="parameters">params style array of parameters to pass into query</param>
		/// <returns>Collection of results from the query</returns>
		public Task<IEnumerable<TEntity>> ExecuteRawQuery( string query, object[] parameters );

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public Task<TEntity> Add( TEntity entity );

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <param name="entities">The entities to insert</param>
		public Task AddRange( IEnumerable<TEntity> entities );

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public Task<TEntity> Update( TEntity entity );

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <param name="entity">The entity to delete</param>
		public Task Delete( TEntity entity );

		/// <summary>
		/// Validates the supplied filters, ensuring they conform to the defined filter criteria
		/// </summary>
		/// <param name="filter">The filter to be validated</param>
		/// <returns></returns>
		public ValidationResult ValidateFilters( FilterChip filter );

		/// <summary>
		/// Execute a query, using the supplied filters, against the underlying datasource
		/// </summary>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public Task<IEnumerable<TEntity>> FindByFilter(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		);
	}
}
