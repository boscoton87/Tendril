using Tendril.Delegates;
using Tendril.InMemory.Services;
using Tendril.InMemory.Services.Interfaces;
using Tendril.Models;
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
		)
			where TModel : class
			where TKey : IComparable {
			var context = CreateDataCollection( dataSource, keyGenerator, findByFilterService );
			dataSource.DataManager.WithDataCollection( context );
			return dataSource;
		}

		/// <summary>
		/// Register a collection with the given DataSourceContext
		/// </summary>
		/// <typeparam name="TView">The type of model presented to consumers of this interface</typeparam>
		/// <typeparam name="TModel">The type of model used internally for CRUD operations</typeparam>
		/// <typeparam name="TKey">The type of key</typeparam>
		/// <param name="dataSource"></param>
		/// <param name="keyGenerator">Service class which defines how keys are managed on the given model type</param>
		/// <param name="findByFilterService">See LinqFindByFilterService class for more information</param>
		/// <param name="convertToModel">Function to convert from the TView to TModel types</param>
		/// <param name="convertToView">Function to convert from the TModel to TView types</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		/// <exception cref="NotSupportedException"></exception>
		public static DataSourceContext<InMemoryDataSource> WithCacheSet<TView, TModel, TKey>(
			this DataSourceContext<InMemoryDataSource> dataSource,
			IKeyGenerator<TModel, TKey> keyGenerator,
			LinqFindByFilterService<TModel> findByFilterService,
			ConvertTo<TView, TModel> convertToModel,
			ConvertTo<TModel, TView> convertToView
		)
			where TView : class
			where TModel : class
			where TKey : IComparable {
			var context = CreateDataCollection( dataSource, keyGenerator, findByFilterService );
			dataSource.DataManager.WithDataCollection( context, convertToModel, convertToView );
			return dataSource;
		}

		private static InMemoryDataCollection<TModel, TKey> CreateDataCollection<TModel, TKey>(
			DataSourceContext<InMemoryDataSource> dataSource, 
			IKeyGenerator<TModel, TKey> keyGenerator,
			LinqFindByFilterService<TModel> findByFilterService
		)
			where TModel : class
			where TKey : IComparable {
			var modelType = typeof( TModel );
			var cache = dataSource.GetDataSource().Cache;
			if ( !cache.ContainsKey( modelType ) ) {
				cache.Add( modelType, new Dictionary<IComparable, object>() );
			}
			var context = new InMemoryDataCollection<TModel, TKey>(
				findByFilterService,
				keyGenerator,
				cache[ modelType ]
			);
			return context;
		}
	}
}
