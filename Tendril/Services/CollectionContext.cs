using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Models;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
	/// <summary>
	/// Class for DataCollection CRUD Operations
	/// </summary>
	public class CollectionContext<TCollection, TDataSource, TModel> : IDataCollection
		where TCollection : class
		where TDataSource : class, IDisposable
		where TModel : class {

		private readonly DataSourceContext<TDataSource> _dataSourceContext;

		private readonly Func<TCollection, TModel, TModel> _add;

		private readonly Action<TCollection, IEnumerable<TModel>> _addRange;

		private readonly Func<TCollection, TModel, TModel> _update;

		private readonly Action<TCollection, TModel> _delete;

		private readonly Func<TDataSource, TCollection> _getCollection;

		private readonly Func<TCollection, string, object[], Task<IEnumerable<TModel>>> _executeRawQuery;

		private readonly Func<FilterChip, ValidationResult> _validateFilters;

		private readonly Func<TCollection, FilterChip, int?, int?, Task<IEnumerable<TModel>>> _findByFilter;

		/// <summary>
		/// Class for DataCollection CRUD Operations
		/// </summary>
		/// <param name="dataSourceContext">The underlying datasource context</param>
		/// <param name="add">Add operation implementation</param>
		/// <param name="addRange">Addrange operation implementation</param>
		/// <param name="update">Update operation implementation</param>
		/// <param name="delete">Delete operation implementation</param>
		/// <param name="getCollection">Get the data collection</param>
		/// <param name="executeRawQuery">Execute raw query operation implementation</param>
		/// <param name="validateFilters">Validate filters function</param>
		/// <param name="findByFilter">Find by filters implementation</param>
		public CollectionContext(
			DataSourceContext<TDataSource> dataSourceContext,
			Func<TCollection, TModel, TModel> add,
			Action<TCollection, IEnumerable<TModel>> addRange,
			Func<TCollection, TModel, TModel> update,
			Action<TCollection, TModel> delete,
			Func<TDataSource, TCollection> getCollection,
			Func<TCollection, string, object[], Task<IEnumerable<TModel>>> executeRawQuery,
			Func<FilterChip, ValidationResult> validateFilters,
			Func<TCollection, FilterChip, int?, int?, Task<IEnumerable<TModel>>> findByFilter
		) {
			_dataSourceContext = dataSourceContext;
			_add = add;
			_addRange = addRange;
			_update = update;
			_delete = delete;
			_getCollection = getCollection;
			_executeRawQuery = executeRawQuery;
			_validateFilters = validateFilters;
			_findByFilter = findByFilter;
		}

		/// <summary>
		/// Execute a raw query against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="query">The query to execute</param>
		/// <param name="parameters">params style array of parameters to pass into query</param>
		/// <returns>Collection of results from the query</returns>
		public async Task<IEnumerable<TEntity>> ExecuteRawQuery<TEntity>( object connection, string query, object[] parameters ) where TEntity : class {
			var result = await _executeRawQuery( _getCollection( connection as TDataSource ), query, parameters );
			return result as IEnumerable<TEntity>;
		}

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public TEntity Add<TEntity>( object connection, TEntity entity ) where TEntity : class {
			return _add( _getCollection( connection as TDataSource ), entity as TModel ) as TEntity;
		}

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entities">The entities to insert</param>
		public void AddRange<TEntity>( object connection, IEnumerable<TEntity> entities ) where TEntity : class {
			_addRange( _getCollection( connection as TDataSource ), entities as IEnumerable<TModel> );
		}

		/// <summary>
		/// Get an instance of the connection
		/// </summary>
		/// <returns>The connection instance</returns>
		public IDisposable GetConnection() {
			return _dataSourceContext.GetDataSource();
		}

		/// <summary>
		/// Apply the pending changes to the datasource
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <returns></returns>
		public Task SaveChangesAsync( object connection ) {
			return _dataSourceContext.SaveChangesAsync( connection as TDataSource );
		}

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public TEntity Update<TEntity>( object connection, TEntity entity ) where TEntity : class {
			return _update( _getCollection( connection as TDataSource ), entity as TModel ) as TEntity;
		}

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="entity">The entity to delete</param>
		public void Delete<TEntity>( object connection, TEntity entity ) where TEntity : class {
			_delete( _getCollection( connection as TDataSource ), entity as TModel );
		}

		/// <summary>
		/// Execute a query with custom conditions against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="connection">The connection</param>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public async Task<ValidationDataResult<IEnumerable<TEntity>>> FindByFilter<TEntity>(
			object connection,
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class {
			var validatedFilters = _validateFilters( filter );
			if ( !validatedFilters.IsSuccess ) {
				return new ValidationDataResult<IEnumerable<TEntity>> {
					IsSuccess = false,
					Message = validatedFilters.Message,
					Data = null
				};
			}
			var results = await _findByFilter( _getCollection( connection as TDataSource ), filter, page, pageSize );
			return new ValidationDataResult<IEnumerable<TEntity>>{ Data = results.Select( v => v as TEntity ).ToList() };
		}
	}
}
