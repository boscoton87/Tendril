using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tendril.Models;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
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

		private readonly Func<TCollection, string, object[], IQueryable<TModel>> _executeRawQuery;

		private readonly Func<FilterChip, ValidationResult> _validateFilters;

		private readonly Func<TCollection, FilterChip, int?, int?, Task<IEnumerable<TModel>>> _findByFilter;

		public CollectionContext(
			DataSourceContext<TDataSource> dataSourceContext,
			Func<TCollection, TModel, TModel> add,
			Action<TCollection, IEnumerable<TModel>> addRange,
			Func<TCollection, TModel, TModel> update,
			Action<TCollection, TModel> delete,
			Func<TDataSource, TCollection> getCollection,
			Func<TCollection, string, object[], IQueryable<TModel>> executeRawQuery,
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

		public async Task<IEnumerable<TEntity>> ExecuteRawQuery<TEntity>( object dataSource, string query, object[] parameters ) where TEntity : class {
			return await _executeRawQuery( _getCollection( dataSource as TDataSource ), query, parameters ).Select( d => d as TEntity ).ToListAsync();
		}

		public TEntity Add<TEntity>( object dataSource, TEntity entity ) where TEntity : class {
			return _add( _getCollection( dataSource as TDataSource ), entity as TModel ) as TEntity;
		}

		public void AddRange<TEntity>( object dataSource, IEnumerable<TEntity> entities ) where TEntity : class {
			_addRange( _getCollection( dataSource as TDataSource ), entities as IEnumerable<TModel> );
		}

		public IDisposable GetDataContext() {
			return _dataSourceContext.GetDataSource();
		}

		public Task SaveChangesAsync( object dataSource ) {
			return _dataSourceContext.SaveChangesAsync( dataSource as TDataSource );
		}

		public TEntity Update<TEntity>( object dataSource, TEntity entity ) where TEntity : class {
			return _update( _getCollection( dataSource as TDataSource ), entity as TModel ) as TEntity;
		}

		public void Delete<TEntity>( object dataSource, TEntity entity ) where TEntity : class {
			_delete( _getCollection( dataSource as TDataSource ), entity as TModel );
		}

		public async Task<ValidationDataResult<IEnumerable<TEntity>>> FindByFilter<TEntity>(
			object dataSource,
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
			var results = await _findByFilter( _getCollection( dataSource as TDataSource ), filter, page, pageSize );
			return new ValidationDataResult<IEnumerable<TEntity>>{ Data = results.Select( v => v as TEntity ).ToList() };
		}
	}
}
