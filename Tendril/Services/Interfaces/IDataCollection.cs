using System.Collections.Generic;
using System.Threading.Tasks;
using Tendril.Models;

namespace Tendril.Services.Interfaces {
	/// <summary>
	/// Interface for DataCollection CRUD Operations
	/// </summary>
	/// <typeparam name="TModel">The type of model</typeparam>
	public interface IDataCollection<TModel>
		where TModel : class {

		/// <summary>
		/// Execute a raw query against the underlying datasource<br />
		/// <b>Note:</b>This operation should only be performed for performance critical code,<br />
		/// or if the required operation is not possible using the other CRUD methods, as this exposes implementation details<br />
		/// </summary>
		/// <param name="query">The query to execute</param>
		/// <param name="parameters">params style array of parameters to pass into query</param>
		/// <returns>Collection of results from the query</returns>
		public Task<IEnumerable<TModel>> ExecuteRawQuery( string query, params object[] parameters );

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public Task<TModel> Add( TModel entity );

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <param name="entities">The entities to insert</param>
		public Task AddRange( IEnumerable<TModel> entities );

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public Task<TModel> Update( TModel entity );

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <param name="entity">The entity to delete</param>
		public Task Delete( TModel entity );

		/// <summary>
		/// Validates the supplied filters, ensuring they conform to the defined filter criteria
		/// </summary>
		/// <param name="filter">The filter to be validated</param>
		/// <returns></returns>
		public ValidationResult ValidateFilters( FilterChip filter );

		/// <summary>
		/// Execute a count query, using the supplied filters, against the underlying datasource
		/// </summary>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public Task<IEnumerable<TModel>> FindByFilter(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		);

		/// <summary>
		/// Execute a query, using the supplied filters, against the underlying datasource
		/// </summary>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public Task<long> CountByFilter(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		);
	}
}
