using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Delegates;
using Tendril.Exceptions;
using Tendril.Models;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
	/// <summary>
	/// Service class which provides a singular CRUD interface to registered data collections
	/// </summary>
	public class DataManager : IDataManager {

		private readonly Dictionary<Type, ICollectionContext> _TViewToCollectionContext = new();

		/// <summary>
		/// Register a new IDataCollection with the DataManager
		/// </summary>
		/// <typeparam name="TModel">The type of internal model</typeparam>
		/// <param name="dataCollection">The IDataCollection to be registered</param>
		public DataManager WithDataCollection<TModel>( IDataCollection<TModel> dataCollection )
			where TModel : class {
			return WithDataCollection( dataCollection, model => model, model => model );
		}

		/// <summary>
		/// Register a new IDataCollection with the DataManager
		/// </summary>
		/// <typeparam name="TView">The type exposed to consumers</typeparam>
		/// <typeparam name="TModel">The type of internal model</typeparam>
		/// <param name="dataCollection">The IDataCollection to be registered</param>
		/// <param name="convertToModel">Function to convert from the TView to TModel types</param>
		/// <param name="convertToView">Function to convert from the TModel to TView types</param>
		/// <returns></returns>
		public DataManager WithDataCollection<TView, TModel>(
			IDataCollection<TModel> dataCollection,
			ConvertTo<TView, TModel> convertToModel,
			ConvertTo<TModel, TView> convertToView
		)
			where TView : class
			where TModel : class {
			var viewType = typeof( TView );
			var context = new CollectionContext<TView, TModel>( dataCollection, convertToModel, convertToView );
			_TViewToCollectionContext.Add( viewType, context );
			return this;
		}

		/// <summary>
		/// Insert a model into the data collection<br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Create a new student with the name <b>John Doe</b><br />
		/// var student = await dataManager.Create( new Student { Name = "John Doe" } );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="source">The model to be inserted</param>
		/// <returns>The model that has been inserted</returns>
		public async Task<TView> Create<TView>( TView source ) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			return await collectionContext.Add( source );
		}

		/// <summary>
		/// Perform a bulk insert into the data collection<br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Create 2 new students with the names <b>John Doe</b> and <b>Jane Doe</b><br />
		/// var students = new List&lt;Student&gt; {
		///		new Student { Name = "John Doe" },
		///		new Student { Name = "Jane Doe" }
		///	};
		/// await dataManager.CreateRange( students, 2 );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="source">The collection of models to be inserted</param>
		/// <param name="batchSize">The number of models to insert at a time. If null, insert in one batch</param>
		public async Task CreateRange<TView>( IEnumerable<TView> source, int? batchSize = null ) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			if ( batchSize.HasValue ) {
				foreach ( var batch in source.Chunk( batchSize.Value ) ) {
					await collectionContext.AddRange( batch );
				}
			} else {
				await collectionContext.AddRange( source );
			}
		}

		/// <summary>
		/// Perform an update on a given model
		/// <br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Insert a student and then change their name<br />
		/// var student = await dataManager.Create( new Student { Name = "John Doe" } );
		/// student.Name = "John Lee Doe";
		/// await dataManager.Update( student );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="source">The model to be updated</param>
		/// <returns>The model that has been updated</returns>
		public async Task<TView> Update<TView>( TView source ) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			return await collectionContext.Update( source );
		}

		/// <summary>
		/// Delete a model from the data collection<br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// await dataManager.Delete( student );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="source">The model to be deleted</param>
		public async Task Delete<TView>( TView source ) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			await collectionContext.Delete( source );
		}

		/// <summary>
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Get all students who's names start with the letter <b>A</b> or <b>B</b><br />
		/// var students = dataManager.FindByFilter&lt;Student&gt;(
		///		new OrFilterChip(
		///			new ValueFilterChip&lt;Student, string&gt;( s =&gt; s.Name, FilterOperator.StartsWith, "A" ),	
		///			new ValueFilterChip&lt;Student, string&gt;( s =&gt; s.Name, FilterOperator.StartsWith, "B" )
		///		)
		/// );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="filter">The filter to use for the query</param>
		/// <param name="page">The page of data to be returned, starting at 0. If null, no pagination will occur</param>
		/// <param name="pageSize">PageSize of data to be returned. If null, no pagination will occur</param>
		/// <returns>Resulting dataset</returns>
		/// <exception cref="UnsupportedFilterException"></exception>
		public async Task<IEnumerable<TView>> FindByFilter<TView>(
			FilterChip filter = null,
			int? page = null,
			int? pageSize = null
		) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			var result = await collectionContext.FindByFilter<TView>( filter, page, pageSize );
			if ( !result.IsSuccess ) {
				throw new UnsupportedFilterException( result.Message );
			}
			return result.Data;
		}

		/// <summary>
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Get count of students who's names start with the letter <b>A</b> or <b>B</b><br />
		/// var studentCount = dataManager.CountByFilter&lt;Student&gt;(
		///		new OrFilterChip(
		///			new ValueFilterChip&lt;Student, string&gt;( s =&gt; s.Name, FilterOperator.StartsWith, "A" ),	
		///			new ValueFilterChip&lt;Student, string&gt;( s =&gt; s.Name, FilterOperator.StartsWith, "B" )
		///		)
		/// );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="filter">The filter to use for the query</param>
		/// <param name="page">The page of data to be returned, starting at 0. If null, no pagination will occur</param>
		/// <param name="pageSize">PageSize of data to be returned. If null, no pagination will occur</param>
		/// <returns>Resulting count of records</returns>
		/// <exception cref="UnsupportedFilterException"></exception>
		public async Task<long> CountByFilter<TView>(
			FilterChip filter = null,
			int? page = null,
			int? pageSize = null
		) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			var result = await collectionContext.CountByFilter<TView>( filter, page, pageSize );
			if ( !result.IsSuccess ) {
				throw new UnsupportedFilterException( result.Message );
			}
			return result.Data;
		}

		/// <summary>
		/// Execute a raw query against the underlying data collection<br />
		/// <b>Note:</b>This operation should only be performed for performance critical code,<br />
		/// or if the required operation is not possible using the other CRUD methods, as this exposes implementation details<br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Using SQL, find all students who's <b>Name</b> contains the substring <b>Lee</b><br />
		/// var students = await dataManager.ExecuteRawQuery&lt;Student&gt;(
		///		@"SELECT * FROM Students s WHERE s.Name LIKE '%' + @name + '%'",
		///		new MySqlParameter( "name", "Lee" )
		/// );
		/// </code>
		/// </summary>
		/// <typeparam name="TView">The type of model</typeparam>
		/// <param name="query">The raw query to execute</param>
		/// <param name="parameters">params style array of query parameters to use</param>
		/// <returns>Resulting dataset</returns>
		public async Task<IEnumerable<TView>> ExecuteRawQuery<TView>( string query, params object[] parameters ) where TView : class {
			var collectionContext = GetCollectionContext<TView>();
			return await collectionContext.ExecuteRawQuery<TView>( query, parameters );
		}

		private ICollectionContext GetCollectionContext<TView>() where TView : class {
			var viewType = typeof( TView );
			if ( !_TViewToCollectionContext.ContainsKey( viewType ) ) {
				throw new TypeLoadException( $"Model type: {viewType} not registered." );
			}
			return _TViewToCollectionContext[ viewType ];
		}
	}
}
