using Tendril.InMemory.Models;
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
	///			.WithDbSet(
	///				s =&gt; s.Id,
	///				( k, s ) =&gt; s.Id = k,
	///				new KeyGeneratorSequential&lt;int&gt;( k => k + 1 ),
	///				new LinqFindByFilterService&lt;Student&gt;()
	///					.WithFilterDefinition( "Id", FilterOperator.EqualTo, v =&gt; s =&gt; s.Id == ( ( int ) v.First() ) ),
	///				new FilterChipValidatorService()
	///					.HasFilterType&lt;int&gt;( "Id", false, 1, 1, FilterOperator.EqualTo )
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
		/// <param name="getKey">Function that defines how to get the key from the model</param>
		/// <param name="setKey">Function that defines how to set the key on the model</param>
		/// <param name="keyGenerator">Service class which supplies new keys to use for storage</param>
		/// <param name="findByFilterService">See LinqFindByFilterService class for more information</param>
		/// <param name="filterChipValidator">If not provided, no filter validation will be performed. 
		/// See FilterChipValidatorService class for more information</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		/// <exception cref="NotSupportedException"></exception>
		public static DataSourceContext<InMemoryDataSource> WithDbSet<TModel, TKey>(
			this DataSourceContext<InMemoryDataSource> dataSource,
			Func<TModel, TKey> getKey,
			Action<TKey, TModel> setKey,
			IKeyGenerator<TModel, TKey> keyGenerator,
			LinqFindByFilterService<TModel> findByFilterService,
			FilterChipValidatorService<TModel>? filterChipValidator = null
		) where TModel : class where TKey : IComparable {
			if ( filterChipValidator is null ) {
				filterChipValidator = new FilterChipValidatorService<TModel>().AllowUndefinedFilters();
			}

			var dataSourceType = typeof( InMemoryDataSource );
			var modelType = typeof( TModel );
			var cache = dataSource.GetDataSource().Cache;
			cache.Add( modelType, new Dictionary<IComparable, object>() );
			var context = new InMemoryDataCollection<TModel, TKey>(
				filterChipValidator,
				findByFilterService,
				getKey,
				setKey,
				keyGenerator,
				cache[ modelType ]
			);
			dataSource.DataManager.WithDataCollection( context );
			return dataSource;
		}
	}
}
