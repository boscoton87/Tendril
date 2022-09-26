using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Delegates;
using Tendril.Models;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
	/// <summary>
	/// Class for DataCollection CRUD Operations
	/// </summary>
	internal class CollectionContext<TView, TModel> : ICollectionContext
		where TView : class
		where TModel : class {

		private readonly IDataCollection<TModel> _dataCollection;

		private readonly ConvertTo<TView, TModel> _convertToModel;

		private readonly ConvertTo<TModel, TView> _convertToView;

		/// <summary>
		/// Class for DataCollection CRUD Operations
		/// </summary>
		public CollectionContext(
			IDataCollection<TModel> dataCollection,
			ConvertTo<TView, TModel> convertToModel,
			ConvertTo<TModel, TView> convertToView
		) {
			_dataCollection = dataCollection;
			_convertToModel = convertToModel;
			_convertToView = convertToView;
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
			return result.Select( r => _convertToView( r ) as TEntity );
		}

		/// <summary>
		/// Execute an insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The entity to insert</param>
		/// <returns>Inserted entity</returns>
		public async Task<TEntity> Add<TEntity>( TEntity entity ) where TEntity : class {
			var result = await _dataCollection.Add( _convertToModel( entity as TView ) );
			return _convertToView( result ) as TEntity;
		}

		/// <summary>
		/// Executes a bulk insert operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entities">The entities to insert</param>
		public async Task AddRange<TEntity>( IEnumerable<TEntity> entities ) where TEntity : class {
			await _dataCollection.AddRange( entities.Select( e => _convertToModel( e as TView ) ) );
		}

		/// <summary>
		/// Execute an update operation against the underyling datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The entity to update</param>
		/// <returns>Updated entity</returns>
		public async Task<TEntity> Update<TEntity>( TEntity entity ) where TEntity : class {
			var result = await _dataCollection.Update( _convertToModel( entity as TView ) );
			return _convertToView( result ) as TEntity;
		}

		/// <summary>
		/// Execute a delete operation against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The entity to delete</param>
		public async Task Delete<TEntity>( TEntity entity ) where TEntity : class {
			await _dataCollection.Delete( _convertToModel( entity as TView ) );
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
			return new ValidationDataResult<IEnumerable<TEntity>> { Data = results.Select( v => _convertToView( v ) as TEntity ).ToList() };
		}

		/// <summary>
		/// Execute a count query with custom conditions against the underlying datasource
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="filter">The filter to apply</param>
		/// <param name="page">The page of data to return, if null then pagination will not occur</param>
		/// <param name="pageSize">The page size of data to return, if null then pagination will not occur</param>
		/// <returns>ValidationDataResult containing the results of the query execution</returns>
		public async Task<ValidationDataResult<long>> CountByFilter<TEntity>(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class {
			var validatedFilters = _dataCollection.ValidateFilters( filter );
			if ( !validatedFilters.IsSuccess ) {
				return new ValidationDataResult<long> {
					IsSuccess = false,
					Message = validatedFilters.Message,
					Data = 0
				};
			}
			var results = await _dataCollection.CountByFilter( filter, page, pageSize );
			return new ValidationDataResult<long> { Data = results };
		}
	}
}
