using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tendril.Models;
using Tendril.Services;

namespace Tendril.EFCore.Extensions {
	public static class EFCoreRegistrationExtensions {
		public static DataSourceContext<TDataSource> WithDbContext<TDataSource>(
			this DataManager dataManager,
			Func<TDataSource> GetDbContext
		) where TDataSource : DbContext {
			var modelType = typeof( TDataSource );
			if ( dataManager.TDataSourceToDataSourceContext.ContainsKey( modelType ) ) {
				throw new ArgumentException( $"Model type: {modelType} already registered." );
			}
			var context = new DataSourceContext<TDataSource> {
				GetDataSource = GetDbContext,
				SaveChangesAsync = context => context.SaveChangesAsync(),
				DataManager = dataManager
			};
			dataManager.TDataSourceToDataSourceContext.Add( modelType, context );
			return context;
		}

		public static DataSourceContext<TDataSource> WithDbSet<TModel, TDataSource>(
			this DataSourceContext<TDataSource> dataSource,
			Func<TDataSource, DbSet<TModel>> getCollection,
			Func<FilterChip, ValidationResult> validateFilters = null,
			Func<DbSet<TModel>, FilterChip, int?, int?, Task<IEnumerable<TModel>>> findByFilter = null
		) where TDataSource : DbContext, IDisposable where TModel : class {
			if ( validateFilters is null ) {
				validateFilters = _ => new ValidationResult();
			}

			var dataSourceType = typeof( TDataSource );
			var modelType = typeof( TModel );
			ValidateDataSourceType( dataSource, dataSourceType );
			var context = new CollectionContext<DbSet<TModel>, TDataSource, TModel>(
				dataSourceContext: dataSource,
				add: ( dbSet, entity ) => dbSet.Add( entity ).Entity,
				addRange: ( dbSet, entities ) => dbSet.AddRange( entities ),
				update: ( dbSet, entity ) => dbSet.Update( entity ).Entity,
				delete: ( dbSet, entity ) => dbSet.Remove( entity ),
				getCollection: getCollection,
				executeRawQuery: ( dbSet, query, parameters ) => dbSet.FromSqlRaw( query, parameters ),
				validateFilters: validateFilters,
				findByFilter: findByFilter
			);
			dataSource.DataManager.TModelToCollectionContext.Add( modelType, context );
			return dataSource;
		}

		private static void ValidateDataSourceType<TDataSource>( DataSourceContext<TDataSource> dataSource, Type dataSourceType )
			where TDataSource : DbContext, IDisposable {
			if ( !dataSource.DataManager.TDataSourceToDataSourceContext.ContainsKey( dataSourceType ) ) {
				throw new TypeLoadException( $"DataSource type: {dataSourceType} not registered." );
			}
		}
	}
}
