using System;
using System.Collections.Generic;
using System.Linq;
using Tendril.Enums;
using Tendril.Models;

namespace Tendril.Services {
	/// <summary>
	/// Service class for defining criteria for FilterChips<br />
	/// <b>Example Usage:</b><br />
	/// <code>
	/// public class Student {
	///		public int Id { get; set; }
	///		public string Name { get; set; }
	///	}
	///	
	/// var validator = new FilterChipValidatorService()
	///		.HasDistinctFields()
	///		.HasFilterType&lt;int&gt;( "Id", false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
	///		.HasFilterType&lt;int&gt;( "Id", false, 1, 10, FilterOperator.In, FilterOperator.NotIn )
	///		.HasFilterType&lt;string&gt;(
	///			"Name", false, 1, 1,
	///			FilterOperator.EqualTo, FilterOperator.NotEqualTo, FilterOperator.StartsWith,
	///			FilterOperator.NotStartsWith, FilterOperator.EndsWith, FilterOperator.NotEndsWith
	///		);
	///	
	/// validator.ValidateFilters( new FilterChip( "Id", FilterOperator.LessThan, 10 ) );
	/// // Fails validation because LessThan is not a defined operator for the <b>Id</b> field
	/// 
	/// validator.ValidateFilters( new FilterChip( "Name", FilterOperator.EqualTo, "John Doe" ) );
	/// // Passes because it meets all criteria defined for the <b>Name</b> field
	///	</code>
	/// </summary>
	public class FilterChipValidatorService {
		private delegate ValidationResult ValidationStep( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit );

		private readonly List<ValidationStep> _validationSteps;

		private bool _allowUndefinedFilters = false;

		private bool _allowNullFilter = true;

		private int? _maxFilterDepth = null;

		public FilterChipValidatorService() {
			_validationSteps = new List<ValidationStep> { ValidateAndOrFilters };
		}

		/// <summary>
		/// ValidateFilters method built using the supplied fluent interface
		/// </summary>
		/// <param name="filter">The FilterChip to be validated</param>
		/// <returns>ValidationResult that states if the filter passed validation, and a message if validation failed</returns>
		public ValidationResult ValidateFilters( FilterChip filter ) {
			if ( filter == null ) {
				if ( !_allowNullFilter )
					return new ValidationResult { IsSuccess = false, Message = "Filter must not be null" };
				return new ValidationResult();
			}
			var filtersWithDepth = FlattenFilterChips( filter );
			if ( _maxFilterDepth.HasValue ) {
				var maxFilterDepthFound = filtersWithDepth.Max( fwd => fwd.Depth );
				if ( maxFilterDepthFound > _maxFilterDepth )
					return new ValidationResult { IsSuccess = false, Message = $"Filter with depth of {maxFilterDepthFound} found, max supported depth is {_maxFilterDepth}" };
			}
			var flattenedFilters = filtersWithDepth.Select( fwd => fwd.Filter ).ToList();
			var filtersHit = flattenedFilters.ToDictionary( f => f, _ => false );
			foreach ( var step in _validationSteps ) {
				var result = step( flattenedFilters, filtersHit );
				if ( !result.IsSuccess )
					return result;
			}
			if ( !_allowUndefinedFilters && filtersHit.Any( f => !f.Value ) )
				return new ValidationResult { IsSuccess = false, Message = "Undefined filter provided" };
			return new ValidationResult();
		}

		/// <summary>
		/// Fail validation if the supplied filter is null
		/// </summary>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		public FilterChipValidatorService RejectNullFilter() {
			_allowNullFilter = false;
			return this;
		}

		/// <summary>
		/// Don't fail validation if any FilterChips are provided that don't map back to a filter defined in the fluent interface
		/// </summary>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		public FilterChipValidatorService AllowUndefinedFilters() {
			_allowUndefinedFilters = true;
			return this;
		}

		/// <summary>
		/// Maximum levels deep that FilterChips can be nested
		/// </summary>
		/// <param name="maxFilterDepth">Max depth of nested filters, must be greater than 0</param>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		/// <exception cref="ArgumentException"></exception>
		public FilterChipValidatorService WithMaxFilterDepth( int maxFilterDepth ) {
			if ( maxFilterDepth < 1 ) {
				throw new ArgumentException( "maxFilterDepth must be greater than or equal to 1" );
			}
			_maxFilterDepth = maxFilterDepth;
			return this;
		}

		/// <summary>
		/// Fail validation if more than one FilterChip maps to a single field
		/// </summary>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		public FilterChipValidatorService HasDistinctFields() {
			ValidationResult step( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit ) {
				if ( filters.Where( f => !IsNestedFilter( f ) ).GroupBy( f => f.Field ).Any( g => g.Count() > 1 ) )
					return new ValidationResult { IsSuccess = false, Message = "Duplicate filter provided" };
				return new ValidationResult();
			}
			_validationSteps.Add( step );
			return this;
		}

		/// <summary>
		/// Define a supported filter type
		/// </summary>
		/// <typeparam name="TValue">The Type used for values on the given filter.<br />
		/// <b>Example:</b> If type string, then the FilterChip.Values Property would be of type string[]</typeparam>
		/// <param name="field">The name of the field this filter will apply to</param>
		/// <param name="required">Whether this filter is required to pass validation</param>
		/// <param name="minValueCount">Min number of values to be supplied on the FilterChip</param>
		/// <param name="maxValueCount">Max number of values to be supplied on the FilterChip</param>
		/// <param name="supportedOperators">params style array of operators supported for this field</param>
		/// <returns>Returns this instance of the class to be chained with the fluent interface</returns>
		/// <exception cref="ArgumentException"></exception>
		public FilterChipValidatorService HasFilterType<TValue>( string field, bool required, int minValueCount, int maxValueCount, params FilterOperator[] supportedOperators ) {
			if ( minValueCount < 1 ) {
				throw new ArgumentException( "minValueCount must be greater than or equal to 1" );
			}
			ValidationResult step( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit ) {
				var foundFilters = filters.Where( f => f != null && f.Field == field && f.Operator.HasValue && supportedOperators.Contains( f.Operator.Value ) ).ToList();
				if ( required && !foundFilters.Any() ) {
					return new ValidationResult { IsSuccess = false, Message = $"{field} filter was not provided" };
				}
				foreach ( var filter in foundFilters ) {
					var values = filter.Values;
					if ( values == null )
						return new ValidationResult { IsSuccess = false, Message = $"{field} filter values must not be null" };
					filtersHit[ filter ] = true;
					if ( values.Length < minValueCount || values.Length > maxValueCount )
						return new ValidationResult {
							IsSuccess = false,
							Message = $"{field} filter values length must be greater than {minValueCount - 1} and less than {maxValueCount + 1}"
						};
					if ( values.Any( v => v is not TValue ) )
						return new ValidationResult {
							IsSuccess = false,
							Message = $"{field} filter values must be of type {typeof( TValue ).Name}"
						};
				}
				return new ValidationResult();
			}
			_validationSteps.Add( step );
			return this;
		}

		private List<FilterWithDepth> FlattenFilterChips( FilterChip filter, int depth = 1 ) {
			var output = new List<FilterWithDepth>();
			if ( IsNestedFilter( filter ) ) {
				var innerDepth = depth + 1;
				foreach ( var innerFilter in filter.Values.Select( v => v as FilterChip ) ) {
					output.AddRange( FlattenFilterChips( innerFilter, innerDepth ) );
				}
			}
			output.Add( new FilterWithDepth { Filter = filter, Depth = depth } );
			return output;
		}

		private bool IsNestedFilter( FilterChip filter ) {
			if ( filter.Values == null )
				return false;
			return filter.Values.Any() && filter.Values.All( v => v is FilterChip );
		}

		private ValidationResult ValidateAndOrFilters( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit ) {
			foreach ( var filter in filters.Where( f => f is AndFilterChip || f is OrFilterChip ) ) {
				if ( !IsNestedFilter( filter ) )
					return new ValidationResult { IsSuccess = false, Message = $"{filter.GetType().Name} values must contain at least 1 FilterChip" };
				filtersHit[ filter ] = true;
			}
			return new ValidationResult();
		}

		private class FilterWithDepth {
			public FilterChip Filter { get; init; }

			public int Depth { get; init; }
		}
	}
}
