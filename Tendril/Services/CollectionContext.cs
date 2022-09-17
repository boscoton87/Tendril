using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Models;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
	/// <summary>
	/// Class for DataCollection CRUD Operations
	/// </summary>
	internal class CollectionContext<TModel> : ICollectionContext
		where TModel : class {

		private readonly IDataCollection<TModel> _dataCollection;

		/// <summary>
		/// Class for DataCollection CRUD Operations
		/// </summary>
		/// <param name="dataCollection">Injected logic for how to perform the CRUD Operations</param>
		public CollectionContext( IDataCollection<TModel> dataCollection ) {
			_dataCollection = dataCollection;
		}

		/// <summary>
		/// Execute a raw query against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="query">The query to execute</param>
		/// <param name="parameters">params style array of parameters to pass into query</param>
		/// <returns>Collection of results from the query</returns>
		public async Task<IEnumerable<TEntity>> ExecuteRawQuery<TEntity>( string query, object[] parameters ) where TEntity : class {
			var result = await _dataCollection.ExecuteRawQuery( query, parameters );
			return result as IEnumerable<TEntity>;
		}

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public async Task<TEntity> Add<TEntity>( TEntity entity ) where TEntity : class {
			return await _dataCollection.Add( entity as TModel ) as TEntity;
		}

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entities">The entities to insert</param>
		public async Task AddRange<TEntity>( IEnumerable<TEntity> entities ) where TEntity : class {
			await _dataCollection.AddRange( entities as IEnumerable<TModel> );
		}

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public async Task<TEntity> Update<TEntity>( TEntity entity ) where TEntity : class {
			return await _dataCollection.Update( entity as TModel ) as TEntity;
		}

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The entity to delete</param>
		public async Task Delete<TEntity>( TEntity entity ) where TEntity : class {
			await _dataCollection.Delete( entity as TModel );
		}

		/// <summary>
		/// Execute a query with custom conditions against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public async Task<ValidationDataResult<IEnumerable<TEntity>>> FindByFilter<TEntity>(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class {
			var validatedFilters = _dataCollection.ValidateFilters( filter );
			if ( !validatedFilters.IsSuccess ) {
				return new ValidationDataResult<IEnumerable<TEntity>> {
					IsSuccess = false,
					Message = validatedFilters.Message,
					Data = null
				};
			}
			var results = await _dataCollection.FindByFilter( filter, page, pageSize );
			return new ValidationDataResult<IEnumerable<TEntity>>{ Data = results.Select( v => v as TEntity ).ToList() };
		}
	}
}
