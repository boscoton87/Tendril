using Microsoft.EntityFrameworkCore;
using Tendril.Models;
using Tendril.Services;

namespace Tendril.EFCore.Extensions {
	/// <summary>
	/// DataManager extension methods for registering an in-memory datasource<br />
	/// <b>Example:</b><br />
	/// <code>
	/// public class Student {
	///		public int Id { get; set; }
	///		public string Name { get; set; }
	/// }
	/// 
	/// public class StudentDbContext : DbContext {
	///		public DbSet&lt;Student&gt; Students { get; set; }
	///	}
	/// 
	/// var findByFilterService = new LinqFindByFilterService&lt;Student&gt;()
	///		.WithFilterDefinition( "Id", FilterOperator.EqualTo, v =&gt; s =&gt; s.Id == ( ( int ) v.First() ) );
	///	
	/// var dataManager = new DataManager()
	///		.WithDbContext( () => new StudentDbContext( new DbContextOptions() ) )
	///			.WithDbSet(
	///				db =&gt; db.Students,
	///				new FilterChipValidatorService()
	///					.HasFilterType&lt;int&gt;( "Id", false, 1, 1, FilterOperator.EqualTo )
	///					.ValidateFilters,
	///				async ( dbSet, filter, page, pageSize ) =&gt;
	///					await findByFilterService.FindByFilter( dbSet, filter, page, pageSize ).ToListAsync()
	///			);
	/// </code>
	/// </summary>
	public static class EFCoreRegistrationExtensions {
		/// <summary>
		/// Specify which DbContext to register a collection to, and how to instantiate it
		/// </summary>
		/// <typeparam name="TDataSource">The type of DbContext</typeparam>
		/// <param name="dataManager"></param>
		/// <param name="GetDbContext">Function that instantiates the specified DbContext type</param>
		/// <returns>Returns an instance of DataSourceContext for registration of collections using the WithDbSet method</returns>
		/// <exception cref="ArgumentException"></exception>
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

		/// <summary>
		/// Register a DbSet with the given DataSourceContext
		/// </summary>
		/// <typeparam name="TModel">The type of entity</typeparam>
		/// <typeparam name="TDataSource">The type of DbContext</typeparam>
		/// <param name="dataSource"></param>
		/// <param name="getCollection">Function that defines how to get the DbSet from the DbContext</param>
		/// <param name="validateFilters">Function for validating filters, see FilterChipValidatorService class for more information</param>
		/// <param name="findByFilter">Function for performing filtering of data, see LinqFindByFilterService class for more information</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		public static DataSourceContext<TDataSource> WithDbSet<TModel, TDataSource>(
			this DataSourceContext<TDataSource> dataSource,
			Func<TDataSource, DbSet<TModel>> getCollection,
			Func<FilterChip, ValidationResult>? validateFilters = null,
			Func<DbSet<TModel>, FilterChip, int?, int?, Task<IEnumerable<TModel>>>? findByFilter = null
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
				executeRawQuery: async ( dbSet, query, parameters ) => await dbSet.FromSqlRaw( query, parameters ).ToListAsync(),
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
