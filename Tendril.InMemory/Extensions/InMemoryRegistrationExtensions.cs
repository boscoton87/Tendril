using Tendril.InMemory.Services;
using Tendril.InMemory.Services.Interfaces;
using Tendril.Services;

namespace Tendril.InMemory.Extensions {
	/// <summary>
	/// DataManager extension methods for registering an in-memory datasource<br />
	/// <b>Example:</b><br />
	/// <code>
	/// public class Student {
	///		public int Id { get; set; }
	///		public string Name { get; set; }
	/// }
	///	
	/// var dataManager = new DataManager()
	///		.WithInMemoryCache()
	///			.WithCacheSet(
	///				new KeyGeneratorSequential&lt;int&gt;(
	///					k =&gt; k + 1,
	///					s =&gt; s.Id,
	///					( k, s ) =&gt; s.Id = k
	///				),
	///				new LinqFindByFilterService&lt;Student&gt;()
	///					.WithFilterType( "Id", FilterOperator.EqualTo, v =&gt; s =&gt; s.Id == ( ( int ) v.First() ) ),
	///			);
	/// </code>
	/// </summary>
	public static class InMemoryRegistrationExtensions {
		/// <summary>
		/// Register a new instance of an in-memory cache
		/// </summary>
		/// <returns>Returns an instance of DataSourceContext for registration of collections using the WithDbSet method</returns>
		/// <exception cref="ArgumentException"></exception>
		public static DataSourceContext<InMemoryDataSource> WithInMemoryCache( this DataManager dataManager ) {
			var dataSource = new InMemoryDataSource();
			var context = new DataSourceContext<InMemoryDataSource> {
				GetDataSource = () => dataSource,
				DataManager = dataManager
			};
			return context;
		}

		/// <summary>
		/// Register a collection with the given DataSourceContext
		/// </summary>
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <typeparam name="TKey">The type of key</typeparam>
		/// <param name="dataSource"></param>
		/// <param name="keyGenerator">Service class which defines how keys are managed on the given model type</param>
		/// <param name="findByFilterService">See LinqFindByFilterService class for more information</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		/// <exception cref="NotSupportedException"></exception>
		public static DataSourceContext<InMemoryDataSource> WithCacheSet<TModel, TKey>(
			this DataSourceContext<InMemoryDataSource> dataSource,
			IKeyGenerator<TModel, TKey> keyGenerator,
			LinqFindByFilterService<TModel> findByFilterService
		) where TModel : class where TKey : IComparable {
			var modelType = typeof( TModel );
			var cache = dataSource.GetDataSource().Cache;
			cache.Add( modelType, new Dictionary<IComparable, object>() );
			var context = new InMemoryDataCollection<TModel, TKey>(
				findByFilterService,
				keyGenerator,
				cache[ modelType ]
			);
			dataSource.DataManager.WithDataCollection( context );
			return dataSource;
		}
	}
}
