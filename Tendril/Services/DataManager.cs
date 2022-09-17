using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Exceptions;
using Tendril.Models;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
	/// <summary>
	/// Service class which provides a singular CRUD interface to registered data collections
	/// </summary>
	public class DataManager : IDataManager {

		private readonly Dictionary<Type, ICollectionContext> _TModelToCollectionContext = new();

		/// <summary>
		/// Register a new IDataCollection with the DataManager
		/// </summary>
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="dataCollection">The IDataCollection to be registered</param>
		public DataManager WithDataCollection<TModel>( IDataCollection<TModel> dataCollection ) where TModel : class {
			var modelType = typeof( TModel );
			var context = new CollectionContext<TModel>( dataCollection );
			_TModelToCollectionContext.Add( modelType, context );
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
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="source">The model to be inserted</param>
		/// <returns>The model that has been inserted</returns>
		public async Task<TModel> Create<TModel>( TModel source ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
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
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="source">The collection of models to be inserted</param>
		/// <param name="batchSize">The number of models to insert at a time. If null, insert in one batch</param>
		public async Task CreateRange<TModel>( IEnumerable<TModel> source, int? batchSize = null ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
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
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="source">The model to be updated</param>
		/// <returns>The model that has been updated</returns>
		public async Task<TModel> Update<TModel>( TModel source ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			return await collectionContext.Update( source );
		}

		/// <summary>
		/// Delete a model from the data collection<br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// await dataManager.Delete( student );
		/// </code>
		/// </summary>
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="source">The model to be deleted</param>
		public async Task Delete<TModel>( TModel source ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
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
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="filter">The filter to use for the query</param>
		/// <param name="page">The page of data to be returned, starting at 0. If null, no pagination will occur</param>
		/// <param name="pageSize">PageSize of data to be returned. If null, no pagination will occur</param>
		/// <returns>Resulting dataset</returns>
		/// <exception cref="UnsupportedFilterException"></exception>
		public async Task<IEnumerable<TModel>> FindByFilter<TModel>(
			FilterChip filter = null,
			int? page = null,
			int? pageSize = null
		) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			var result = await collectionContext.FindByFilter<TModel>( filter, page, pageSize );
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
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="query">The raw query to execute</param>
		/// <param name="parameters">params style array of query parameters to use</param>
		/// <returns>Resulting dataset</returns>
		public async Task<IEnumerable<TModel>> ExecuteRawQuery<TModel>( string query, params object[] parameters ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			return await collectionContext.ExecuteRawQuery<TModel>( query, parameters );
		}

		private ICollectionContext GetCollectionContext<TModel>() where TModel : class {
			var modelType = typeof( TModel );
			if ( !_TModelToCollectionContext.ContainsKey( modelType ) ) {
				throw new TypeLoadException( $"Model type: {modelType} not registered." );
			}
			return _TModelToCollectionContext[ modelType ];
		}
	}
}
