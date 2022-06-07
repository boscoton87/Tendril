using System;
using System.Collections.Generic;
using System.Linq;
using Tendril.Enums;

namespace Tendril.Models {
	public class FilterChipValidator {
		private delegate ValidationResult ValidationStep( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit );

		private readonly List<ValidationStep> _validationSteps;

		private bool _allowUndefinedFilters = false;

		private bool _allowNullFilter = true;

		private int? _maxFilterDepth = null;

		public FilterChipValidator() {
			_validationSteps = new List<ValidationStep> { ValidateAndOrFilters };
		}

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

		public FilterChipValidator RejectNullFilter() {
			_allowNullFilter = false;
			return this;
		}

		public FilterChipValidator AllowUndefinedFilters() {
			_allowUndefinedFilters = true;
			return this;
		}

		public FilterChipValidator WithMaxFilterDepth( int maxFilterDepth ) {
			if ( maxFilterDepth < 1 ) {
				throw new ArgumentException( "maxFilterDepth must be greater than or equal to 1" );
			}
			_maxFilterDepth = maxFilterDepth;
			return this;
		}

		public FilterChipValidator HasDistinctFields() {
			ValidationResult step( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit ) {
				if ( filters.Where( f => !IsNestedFilter( f ) ).GroupBy( f => f.Field ).Any( g => g.Count() > 1 ) )
					return new ValidationResult { IsSuccess = false, Message = "Duplicate filter provided" };
				return new ValidationResult();
			}
			_validationSteps.Add( step );
			return this;
		}

		public FilterChipValidator HasFilterType<TValue>( string field, bool required, int minValueCount, int maxValueCount, params FilterOperator[] supportedOperators ) {
			if ( minValueCount < 1 ) {
				throw new ArgumentException( "minValueCount must be greater than or equal to 1" );
			}
			ValidationResult step( IEnumerable<FilterChip> filters, Dictionary<FilterChip, bool> filtersHit ) {
				var foundFilters = filters.Where( f => f != null && f.Field == field && supportedOperators.Contains( f.Operator ) ).ToList();
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
