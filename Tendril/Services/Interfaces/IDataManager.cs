using System.Collections.Generic;
using System.Threading.Tasks;
using Tendril.Models;
using Tendril.Exceptions;
using System.Linq;

namespace Tendril.Services.Interfaces {
	/// <summary>
	/// Interface for service class which provides a CRUD interface to registered data collections
	/// </summary>
	public interface IDataManager {

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
		Task<TModel> Create<TModel>( TModel source ) where TModel : class;

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
		/// <param name="batchSize">The number of models to insert at a time</param>
		Task CreateRange<TModel>( IEnumerable<TModel> source, int batchSize ) where TModel : class;

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
		Task<TModel> Update<TModel>( TModel source ) where TModel : class;

		/// <summary>
		/// Delete a model from the data collection<br />
		/// <b>Example Usage:</b><br />
		/// <code>
		/// await dataManager.Delete( student );
		/// </code>
		/// </summary>
		/// <typeparam name="TModel">The type of model</typeparam>
		/// <param name="source">The model to be deleted</param>
		Task Delete<TModel>( TModel source ) where TModel : class;

		/// <summary>
		/// <b>Example Usage:</b><br />
		/// <code>
		/// // Get all students who's names start with the letter <b>A</b> or <b>B</b><br />
		/// var students = dataManager.FindByFilter&lt;Student&gt;(
		///		new OrFilterChip(
		///			new FilterChip( "Name", FilterOperator.StartsWith, "A" ),	
		///			new FilterChip( "Name", FilterOperator.StartsWith, "B" )
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
		Task<IEnumerable<TModel>> FindByFilter<TModel>(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TModel : class;

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
		IQueryable<TModel> ExecuteRawQuery<TModel>( string query, params object[] parameters ) where TModel : class;
	}
}