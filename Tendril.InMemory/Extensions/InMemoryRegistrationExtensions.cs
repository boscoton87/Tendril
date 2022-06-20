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
	/// var findByFilterService = new LinqFindByFilterService&lt;Student&gt;()
	///		.WithFilterDefinition( "Id", FilterOperator.EqualTo, v =&gt; s =&gt; s.Id == ( ( int ) v.First() ) );
	///	
	/// var dataManager = new DataManager()
	///		.WithInMemoryCache()
	///			.WithDbSet(
	///				s =&gt; s.Id,
	///				( k, s ) =&gt; s.Id = k,
	///				new KeyGeneratorSequential&lt;int&gt;( k => k + 1 ),
	///				new FilterChipValidatorService()
	///					.HasFilterType&lt;int&gt;( "Id", false, 1, 1, FilterOperator.EqualTo )
	///					.ValidateFilters,
	///				( cache, filter, page, pageSize ) =&gt; Task.FromResult(
	///					findByFilterService.FindByFilter(
	///						cache.Values.Select( v =&gt; v as Student ).AsQueryable(),
	///						filter,
	///						page,
	///						pageSize
	///					).AsEnumerable()
	///				)
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

		/// <summary>
		/// Register a collection with the given DataSourceContext
		/// </summary>
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <typeparam name="TKey">The type of key</typeparam>
		/// <param name="getKey">Function that defines how to get the key from the model</param>
		/// <param name="setKey">Function that defines how to set the key on the model</param>
		/// <param name="keyGenerator">Service class which supplies new keys to use for storage</param>
		/// <param name="validateFilters">Function for validating filters, see FilterChipValidatorService class for more information</param>
		/// <param name="findByFilter">Function for performing filtering of data, se LinqFindByFilterService class for more information</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		/// <exception cref="NotSupportedException"></exception>
		public static DataSourceContext<InMemoryDataSource> WithDbSet<TModel, TKey>(
			this DataSourceContext<InMemoryDataSource> dataSource,
			Func<TModel, TKey> getKey,
			Action<TKey, TModel> setKey,
			IKeyGenerator<TModel, TKey> keyGenerator,
			Func<FilterChip, ValidationResult>? validateFilters = null,
			Func<Dictionary<IComparable, object>, FilterChip, int?, int?, Task<IEnumerable<TModel>>>? findByFilter = null
		) where TModel : class where TKey : IComparable {
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
					setKey( keyGenerator.GetNextKey( entity ), entity );
					var key = getKey( entity );
					dbSet.Add( key, entity );
					return entity;
				},
				addRange: ( dbSet, entities ) => {
					foreach ( var entity in entities ) {
						setKey( keyGenerator.GetNextKey( entity ), entity );
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
