using Microsoft.EntityFrameworkCore;
using Tendril.EFCore.Models;
using Tendril.Services;

namespace Tendril.EFCore.Extensions {
	/// <summary>
	/// DataManager extension methods for registering an EFCore DbContext<br />
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
	///		.WithFilterDefinition( s =&gt; s.Id, FilterOperator.EqualTo, v =&gt; s =&gt; s.Id == v.First() );
	///	
	/// var dataManager = new DataManager()
	///		.WithDbContext( () => new StudentDbContext( new DbContextOptions() ) )
	///			.WithDbSet(
	///				db =&gt; db.Students,
	///				new FilterChipValidatorService()
	///					.HasFilterType&lt;int&gt;( "Id", false, 1, 1, FilterOperator.EqualTo )
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
		public static DataSourceContext<TDataSource> WithDbContext<TDataSource>(
			this DataManager dataManager,
			Func<TDataSource> GetDbContext
		) where TDataSource : DbContext {
			var context = new DataSourceContext<TDataSource> {
				GetDataSource = GetDbContext,
				DataManager = dataManager
			};
			return context;
		}

		/// <summary>
		/// Register a DbSet with the given DataSourceContext
		/// </summary>
		/// <typeparam name="TModel">The type of entity</typeparam>
		/// <typeparam name="TDataSource">The type of DbContext</typeparam>
		/// <param name="dataSource"></param>
		/// <param name="getCollection">Function that defines how to get the DbSet from the DbContext</param>
		/// <param name="findByFilterService">See LinqFindByFilterService class for more information</param>
		/// <param name="filterChipValidator">If not provided, no filter validation will be performed. 
		/// See FilterChipValidatorService class for more information</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		public static DataSourceContext<TDataSource> WithDbSet<TModel, TDataSource>(
			this DataSourceContext<TDataSource> dataSource,
			Func<TDataSource, DbSet<TModel>> getCollection,
			LinqFindByFilterService<TModel> findByFilterService,
			FilterChipValidatorService<TModel>? filterChipValidator = null
		) where TDataSource : DbContext, IDisposable where TModel : class {
			if ( filterChipValidator is null ) {
				filterChipValidator = new FilterChipValidatorService<TModel>().AllowUndefinedFilters();
			}
			var modelType = typeof( TModel );
			var context = new EFCoreDataCollection<TDataSource, TModel>(
				filterChipValidator,
				findByFilterService,
				dataSource,
				getCollection
			);
			dataSource.DataManager.WithDataCollection( context );
			return dataSource;
		}
	}
}
