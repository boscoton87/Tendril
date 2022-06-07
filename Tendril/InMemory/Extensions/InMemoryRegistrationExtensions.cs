using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.InMemory.Models;
using Tendril.Models;
using Tendril.Services;

namespace Tendril.InMemory.Extensions {
	public static class InMemoryRegistrationExtensions {
		public static DataSourceContext<InMemoryDataSource> WithInMemoryCache( this DataManager dataManager ) {
			var modelType = typeof( InMemoryDataSource );
			if ( dataManager.TDataSourceToDataSourceContext.ContainsKey( modelType ) ) {
				throw new ArgumentException( $"Model type: {modelType} already registered." );
			}
			var dataSource = new InMemoryDataSource();
			var context = new DataSourceContext<InMemoryDataSource> {
				GetDataSource = () => dataSource,
				SaveChangesAsync = _ => Task.CompletedTask,
				DataManager = dataManager
			};
			dataManager.TDataSourceToDataSourceContext.Add( modelType, context );
			return context;
		}

		public static DataSourceContext<InMemoryDataSource> WithDbSet<TModel>(
			this DataSourceContext<InMemoryDataSource> dataSource,
			Func<TModel, IComparable> getKey,
			Action<TModel> setKey,
			Func<FilterChip, ValidationResult> validateFilters = null,
			Func<Dictionary<IComparable, object>, FilterChip, int?, int?, Task<IEnumerable<TModel>>> findByFilter = null
		) where TModel : class {
			if ( validateFilters is null ) {
				validateFilters = _ => new ValidationResult();
			}

			var dataSourceType = typeof( InMemoryDataSource );
			var modelType = typeof( TModel );
			ValidateDataSourceType( dataSource, dataSourceType );
			dataSource.GetDataSource().Cache.Add( modelType, new Dictionary<IComparable, object>() );
			var context = new CollectionContext<Dictionary<IComparable, object>, InMemoryDataSource, TModel>(
				dataSourceContext: dataSource,
				add: ( dbSet, entity ) => {
					setKey( entity );
					var key = getKey( entity );
					dbSet.Add( key, entity );
					return entity;
				},
				addRange: ( dbSet, entities ) => {
					foreach ( var entity in entities ) {
						setKey( entity );
						var key = getKey( entity );
						dbSet.Add( key, entity );
					}
				},
				update: ( dbSet, entity ) => {
					var key = getKey( entity );
					_ = dbSet.Keys.Single( k => k.CompareTo( key ) == 0 );
					dbSet[ key ] = entity;
					return entity;
				},
				delete: ( dbSet, entity ) => {
					var key = getKey( entity );
					dbSet.Remove( key );
				},
				getCollection: datasource => datasource.Cache[ modelType ],
				executeRawQuery: ( dbSet, query, parameters ) => throw new NotSupportedException(),
				validateFilters: validateFilters,
				findByFilter: findByFilter
			);
			dataSource.DataManager.TModelToCollectionContext.Add( modelType, context );
			return dataSource;
		}

		private static void ValidateDataSourceType( DataSourceContext<InMemoryDataSource> dataSource, Type dataSourceType ) {
			if ( !dataSource.DataManager.TDataSourceToDataSourceContext.ContainsKey( dataSourceType ) ) {
				throw new TypeLoadException( $"DataSource type: {dataSourceType} not registered." );
			}
		}
	}
}
