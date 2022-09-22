using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Tendril.Enums;
using Tendril.Exceptions;
using Tendril.Extensions;
using Tendril.Models;

namespace Tendril.Services {
	/// <summary>
	/// Service class for defining how FilterChips will be converted into a linq query<br />
	/// <b>Example Usage:</b><br />
	///	<code>
	///	public class Student {
	///		public int Id { get; set; }
	///		public string Name { get; set; }
	///	}
	///	
	/// var filterService = new LinqFindByFilterService&lt;Student&gt;()
	///		.WithFilterType( s =&gt; s.Id, FilterOperator.EqualTo, values => s => s.Id == values.First() )
	///		.WithFilterType( s =&gt; s.Id, FilterOperator.NotEqualTo, values => s => s.Id != values.First() )
	///		.WithFilterType( s =&gt; s.Name, FilterOperator.StartsWith, values => s => s.Name.StartsWith( v.Single() ) )
	///		.WithFilterType( s =&gt; s.Name, FilterOperator.EndsWith, values => s => s.Name.EndsWith( v.Single() ) );
	///	
	/// var data = new List&lt;Student&gt; {
	///		new Student { Name = "John Lee Doe" },
	///		new Student { Name = "Jane Sue Doe" }
	///	}.AsQueryable();
	/// 
	/// var filters = new AndFilterChip(
	///		new ValueFilterChip&lt;Student, string&gt;( s =&gt; s.Name, FilterOperator.StartsWith, "John" ),
	///		new ValueFilterChip&lt;Student, string&gt;( s =&gt; s.Name, FilterOperator.EndsWith, "Doe" )
	/// );
	/// 
	/// var results = filterService.FindByFilter( data, filters );
	/// // results just contains the Student with name <b>John Lee Doe</b>
	/// 
	/// </code>
	/// </summary>
	/// <typeparam name="TModel">The type of model</typeparam>
	public class LinqFindByFilterService<TModel> where TModel : class {
		/// <summary>
		/// Delegate that defines the shape for converting FilterChip value object arrays into a linq expression<br />
		/// </summary>
		/// <param name="filterValues">Values property of a FilterChip</param>
		/// <returns>Linq Expression to be applied to a Dataset</returns>
		private delegate Expression<Func<TModel, bool>> FilterByValues( object[] filterValues );

		private readonly Dictionary<(string field, FilterOperator filterOperator), FilterByValues> _fieldToExpressionBuilder = new();

		private readonly FilterChipValidatorService<TModel> _validator = new();

		/// <summary>
		/// ValidateFilters method built using the supplied fluent interface
		/// </summary>
		/// <param name="filter">The FilterChip to be validated</param>
		/// <returns>ValidationResult that states if the filter passed validation, and a message if validation failed</returns>
		public ValidationResult ValidateFilters( FilterChip filter ) {
			return _validator.ValidateFilters( filter );
		}

		/// <summary>
		/// Fail validation if the supplied filter is null
		/// </summary>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		public LinqFindByFilterService<TModel> RejectNullFilter() {
			_validator.RejectNullFilter();
			return this;
		}

		/// <summary>
		/// Maximum levels deep that FilterChips can be nested
		/// </summary>
		/// <param name="maxFilterDepth">Max depth of nested filters, must be greater than 0</param>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		/// <exception cref="ArgumentException"></exception>
		public LinqFindByFilterService<TModel> WithMaxFilterDepth( int maxFilterDepth ) {
			_validator.WithMaxFilterDepth( maxFilterDepth );
			return this;
		}

		/// <summary>
		/// Fail validation if more than one FilterChip maps to a single field
		/// </summary>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		public LinqFindByFilterService<TModel> HasDistinctFields() {
			_validator.HasDistinctFields();
			return this;
		}

		/// <summary>
		/// Fluent interface for defining how a field/operator combination will be converted into a linq expression
		/// </summary>
		/// <param name="getProperty">Expression to get the name of the field that this definition will apply to</param>
		/// <param name="filterOperator">The operator that this definition will apply to</param>
		/// <param name="filterByValues">The expression building function that will be tied to this field/operator combination</param>
		/// <param name="required">Whether this filter is required to pass validation</param>
		/// <param name="minValueCount">Min number of values to be supplied on the FilterChip</param>
		/// <param name="maxValueCount">Max number of values to be supplied on the FilterChip</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		public LinqFindByFilterService<TModel> WithFilterType<TValue>(
			Expression<Func<TModel, TValue>> getProperty,
			FilterOperator filterOperator,
			Func<TValue[], Expression<Func<TModel, bool>>> filterByValues,
			bool required = false,
			int minValueCount = 1,
			int maxValueCount = 1
		) {
			return WithFilterType( getProperty.GetPropertyName(), filterOperator, filterByValues, required, minValueCount, maxValueCount );
		}

		/// <summary>
		/// Fluent interface for defining how a field/operator combination will be converted into a linq expression
		/// </summary>
		/// <param name="field">The name of the field that this definition will apply to</param>
		/// <param name="filterOperator">The operator that this definition will apply to</param>
		/// <param name="filterByValues">The expression building function that will be tied to this field/operator combination</param>
		/// <param name="required">Whether this filter is required to pass validation</param>
		/// <param name="minValueCount">Min number of values to be supplied on the FilterChip</param>
		/// <param name="maxValueCount">Max number of values to be supplied on the FilterChip</param>
		/// <returns>Returns this instance of the class to be chained with Fluent calls of this method</returns>
		public LinqFindByFilterService<TModel> WithFilterType<TValue>(
			string field,
			FilterOperator filterOperator,
			Func<TValue[], Expression<Func<TModel, bool>>> filterByValues,
			bool required = false,
			int minValueCount = 1,
			int maxValueCount = 1
		) {
			_validator.HasFilterType<TValue>( field, required, minValueCount, maxValueCount, filterOperator );
			FilterByValues filterToStore = values => filterByValues( values.Select( v => ( TValue ) v ).ToArray() );
			_fieldToExpressionBuilder.Add( (field, filterOperator), filterToStore );
			return this;
		}

		/// <summary>
		/// FindByFilter method built using supplied definitions from fluent interface<br />
		/// </summary>
		/// <param name="dbSet">Dataset to be filtered</param>
		/// <param name="filter">Filter containing query criteria</param>
		/// <param name="page">Page of data to be returned, starting at 0. If null, no pagination will occur</param>
		/// <param name="pageSize">PageSize of data to be returned. If null, no pagination will occur</param>
		/// <returns>Dataset with filtering applied to it</returns>
		/// <exception cref="UnsupportedFilterException"></exception>
		public IQueryable<TModel> FindByFilter( IQueryable<TModel> dbSet, FilterChip filter, int? page = null, int? pageSize = null ) {
			var query = dbSet.AsQueryable();
			if ( filter is not null ) {
				var predicate = BuildPredicate( filter );
				query = query.Where( predicate );
			}
			if ( ( page.HasValue && page.Value >= 0 ) && ( pageSize.HasValue && pageSize.Value >= 0 ) ) {
				query = query.Skip( page.Value * pageSize.Value ).Take( pageSize.Value );
			}
			return query;
		}

		private Expression<Func<TModel, bool>> BuildPredicate( FilterChip filter ) {
			if ( filter is AndFilterChip ) {
				return AndAll( filter.Values.Select( v => BuildPredicate( v as FilterChip ) ).ToArray() );
			} else if ( filter is OrFilterChip ) {
				return OrAny( filter.Values.Select( v => BuildPredicate( v as FilterChip ) ).ToArray() );
			} else if (
				filter.Operator.HasValue
				&& _fieldToExpressionBuilder.TryGetValue(
					(filter.Field, filter.Operator.Value)
					, out FilterByValues filterHandler
				)
			) {
				return filterHandler( filter.Values );
			} else
				throw new UnsupportedFilterException( $"No {filter.Field} filter definition found for {( filter.Operator.HasValue ? filter.Operator : "null" )} operator" );
		}

		private Expression<Func<TModel, bool>> AndAll( params Expression<Func<TModel, bool>>[] expressions ) {
			if ( expressions == null ) {
				throw new ArgumentNullException( nameof( expressions ) );
			}
			if ( !expressions.Any() ) {
				return t => true;
			}
			var firstPass = true;
			Expression<Func<TModel, bool>> output = null;
			foreach ( var expr in expressions ) {
				if ( firstPass ) {
					output = expr;
					firstPass = false;
				} else {
					var invokedExpr = Expression.Invoke( expr, output.Parameters.Cast<Expression>() );
					output = Expression.Lambda<Func<TModel, bool>>( Expression.AndAlso( output.Body, invokedExpr ), output.Parameters );
				}
			}
			return output;
		}

		private Expression<Func<TModel, bool>> OrAny( params Expression<Func<TModel, bool>>[] expressions ) {
			if ( expressions == null ) {
				throw new ArgumentNullException( nameof( expressions ) );
			}
			if ( !expressions.Any() ) {
				return t => true;
			}
			var firstPass = true;
			Expression<Func<TModel, bool>> output = null;
			foreach ( var expr in expressions ) {
				if ( firstPass ) {
					output = expr;
					firstPass = false;
				} else {
					var invokedExpr = Expression.Invoke( expr, output.Parameters.Cast<Expression>() );
					output = Expression.Lambda<Func<TModel, bool>>( Expression.OrElse( output.Body, invokedExpr ), output.Parameters );
				}
			}
			return output;
		}
	}
}
