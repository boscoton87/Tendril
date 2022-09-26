using Microsoft.EntityFrameworkCore;
using Tendril.Delegates;
using Tendril.EFCore.Delegates;
using Tendril.EFCore.Services;
using Tendril.Models;
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
	/// var dataManager = new DataManager()
	///		.WithDbContext( () => new StudentDbContext( new DbContextOptions() ) )
	///			.WithDbSet(
	///				db =&gt; db.Students,
	///				new LinqFindByFilterService&lt;Student&gt;()
	///					.WithFilterType( s =&gt; s.Id, FilterOperator.EqualTo, v =&gt; s =&gt; s.Id == v.First() )
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
		/// <param name="getQueryableCollection">Function that defines how to get the DbSet when loading from collection, if null then use getCollection for loading</param>
		/// <param name="addOverride">Optionally override the Add operation for this collection</param>
		/// <param name="addRangeOverride">Optionally override the AddRange operation for this collection</param>
		/// <param name="deleteOverride">Optionally override the Delete operation for this collection</param>
		/// <param name="executeRawQueryOverride">Optionally override the ExecuteRawQuery operation for this collection</param>
		/// <param name="updateOverride">Optionally override the Update operation for this collection</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		public static DataSourceContext<TDataSource> WithDbSet<TModel, TDataSource>(
			this DataSourceContext<TDataSource> dataSource,
			GetDbSet<TDataSource, TModel> getCollection,
			LinqFindByFilterService<TModel> findByFilterService,
			GetCollectionQueryable<TDataSource, TModel>? getQueryableCollection = null,
			AddEntity<TDataSource, TModel>? addOverride = null,
			AddEntities<TDataSource, TModel>? addRangeOverride = null,
			DeleteEntity<TDataSource, TModel>? deleteOverride = null,
			FindByRawQuery<TDataSource, TModel>? executeRawQueryOverride = null,
			UpdateEntity<TDataSource, TModel>? updateOverride = null
		) where TDataSource : DbContext, IDisposable where TModel : class {
			var context = new EFCoreDataCollection<TDataSource, TModel>(
				findByFilterService,
				dataSource,
				getCollection,
				getQueryableCollection,
				addOverride,
				addRangeOverride,
				deleteOverride,
				executeRawQueryOverride,
				updateOverride
			);
			dataSource.DataManager.WithDataCollection( context );
			return dataSource;
		}

		/// <summary>
		/// Register a DbSet with the given DataSourceContext
		/// </summary>
		/// <typeparam name="TView">The type of model presented to consumers of this interface</typeparam>
		/// <typeparam name="TModel">The type of model used internally for CRUD operations</typeparam>
		/// <typeparam name="TDataSource">The type of DbContext</typeparam>
		/// <param name="dataSource"></param>
		/// <param name="getCollection">Function that defines how to get the DbSet from the DbContext</param>
		/// <param name="findByFilterService">See LinqFindByFilterService class for more information</param>
		/// <param name="convertToModel">Function to convert from the TView to TModel types</param>
		/// <param name="convertToView">Function to convert from the TModel to TView types</param>
		/// <param name="getQueryableCollection">Function that defines how to get the DbSet when loading from collection, if null then use getCollection for loading</param>
		/// <param name="addOverride">Optionally override the Add operation for this collection</param>
		/// <param name="addRangeOverride">Optionally override the AddRange operation for this collection</param>
		/// <param name="deleteOverride">Optionally override the Delete operation for this collection</param>
		/// <param name="executeRawQueryOverride">Optionally override the ExecuteRawQuery operation for this collection</param>
		/// <param name="updateOverride">Optionally override the Update operation for this collection</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		public static DataSourceContext<TDataSource> WithDbSet<TView, TModel, TDataSource>(
			this DataSourceContext<TDataSource> dataSource,
			GetDbSet<TDataSource, TModel> getCollection,
			LinqFindByFilterService<TModel> findByFilterService,
			ConvertTo<TView, TModel> convertToModel,
			ConvertTo<TModel, TView> convertToView,
			GetCollectionQueryable<TDataSource, TModel>? getQueryableCollection = null,
			AddEntity<TDataSource, TModel>? addOverride = null,
			AddEntities<TDataSource, TModel>? addRangeOverride = null,
			DeleteEntity<TDataSource, TModel>? deleteOverride = null,
			FindByRawQuery<TDataSource, TModel>? executeRawQueryOverride = null,
			UpdateEntity<TDataSource, TModel>? updateOverride = null
		)
			where TView : class
			where TModel : class
			where TDataSource : DbContext, IDisposable {
			var context = new EFCoreDataCollection<TDataSource, TModel>(
				findByFilterService,
				dataSource,
				getCollection,
				getQueryableCollection,
				addOverride,
				addRangeOverride,
				deleteOverride,
				executeRawQueryOverride,
				updateOverride
			);
			dataSource.DataManager.WithDataCollection( context, convertToModel, convertToView );
			return dataSource;
		}
	}
}
