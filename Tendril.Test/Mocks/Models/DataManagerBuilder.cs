using System.Linq.Expressions;
using Tendril.Enums;
using Tendril.InMemory.Extensions;
using Tendril.Models;
using Tendril.Services;

namespace Tendril.Test.Mocks.Models {
	internal static class DataManagerBuilder {
		private delegate Expression<Func<Student, bool>> FilterStudents( FilterChip filter );

		public static DataManager BuildDataManager() {
			var nextKey = 1;
			var dataManager = new DataManager();
			dataManager
				.WithInMemoryCache()
					.WithDbSet(
						s => s.Id,
						s => s.Id = nextKey++,
						new FilterChipValidator()
							.HasDistinctFields()
							.HasFilterType<int>( "Id", false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
							.HasFilterType<int>( "Id", false, 1, 10, FilterOperator.Contains, FilterOperator.NotContains )
							.HasFilterType<string>(
								"Name", false, 1, 1,
								FilterOperator.EqualTo, FilterOperator.NotEqualTo, FilterOperator.StartsWith,
								FilterOperator.NotStartsWith, FilterOperator.EndsWith, FilterOperator.NotEndsWith
							)
							.HasFilterType<bool>( "IsEnrolled", false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
							.HasFilterType<DateTime>(
								"DateOfBirth", false, 1, 1,
								FilterOperator.EqualTo, FilterOperator.NotEqualTo, FilterOperator.LessThan, FilterOperator.LessThanOrEqualTo,
								FilterOperator.GreaterThan, FilterOperator.GreaterThanOrEqualTo
							)
							.ValidateFilters,
						FindByFilter
					);
			return dataManager;
		}

		private static Task<IEnumerable<Student>> FindByFilter( Dictionary<IComparable, object> cache, FilterChip filter, int? page, int? pageSize ) {
			var query = cache.Values.Select( v => v as Student );
			if ( filter is not null )
				query = query.Where( BuildPredicate( filter ).Compile() );
			if ( page.HasValue && pageSize.HasValue ) {
				query = query.Skip( page.Value * pageSize.Value ).Take( pageSize.Value );
			}
			return Task.FromResult( query );
		}

		private static Expression<Func<Student, bool>> BuildPredicate( FilterChip filter ) {
			var filterHandlers = new Dictionary<string, FilterStudents> {
				{ "Id", FilterById },
				{ "Name", FilterByName },
				{ "IsEnrolled", FilterByIsEnrolled },
				{ "DateOfBirth", FilterByDateOfBirth },
			};

			if ( filter is AndFilterChip ) {
				Expression<Func<Student, bool>> predicate = s => true;
				foreach ( var innerFilter in filter.Values.Select( v => v as FilterChip ) ) {
					var newPredicate = BuildPredicate( innerFilter );
					predicate = s => predicate.Compile()( s ) && newPredicate.Compile()( s );
				}
				return predicate;
			} else if ( filter is OrFilterChip ) {
				Expression<Func<Student, bool>> predicate = s => true;
				foreach ( var innerFilter in filter.Values.Select( v => v as FilterChip ) ) {
					var newPredicate = BuildPredicate( innerFilter );
					predicate = s => predicate.Compile()( s ) || newPredicate.Compile()( s );
				}
				return predicate;
			} else if ( filterHandlers.TryGetValue( filter.Field, out FilterStudents filterHandler ) )
				return filterHandler( filter );
			else
				throw new Exception( "Unsupported filter" );
		}

		private static Expression<Func<Student, bool>> FilterById( FilterChip filter ) {
			switch ( filter.Operator ) {
				case FilterOperator.EqualTo:
					return s => s.Id == ( ( int ) filter.Values.First() );
				case FilterOperator.NotEqualTo:
					return s => s.Id != ( ( int ) filter.Values.First() );
				case FilterOperator.Contains:
					var values = filter.Values.Select( v => ( int ) v );
					return s => values.Contains( s.Id );
				case FilterOperator.NotContains:
					values = filter.Values.Select( v => ( int ) v );
					return s => !values.Contains( s.Id );
				default:
					throw new Exception( "Unsupported filter" );
			}
		}

		private static Expression<Func<Student, bool>> FilterByName( FilterChip filter ) {
			var name = filter.Values.Single() as string;
			return filter.Operator switch {
				FilterOperator.EqualTo => s => s.Name == name,
				FilterOperator.NotEqualTo => s => s.Name != name,
				FilterOperator.StartsWith => s => s.Name.StartsWith( name ),
				FilterOperator.NotStartsWith => s => !s.Name.StartsWith( name ),
				FilterOperator.EndsWith => s => s.Name.EndsWith( name ),
				FilterOperator.NotEndsWith => s => !s.Name.EndsWith( name ),
				_ => throw new Exception( "Unsupported filter" ),
			};
		}

		private static Expression<Func<Student, bool>> FilterByIsEnrolled( FilterChip filter ) {
			var isEnrolled = ( bool ) filter.Values.Single();
			return filter.Operator switch {
				FilterOperator.EqualTo => s => s.IsEnrolled == isEnrolled,
				FilterOperator.NotEqualTo => s => s.IsEnrolled != isEnrolled,
				_ => throw new Exception( "Unsupported filter" ),
			};
		}

		private static Expression<Func<Student, bool>> FilterByDateOfBirth( FilterChip filter ) {
			var dateOfBirth = ( DateTime ) filter.Values.Single();
			return filter.Operator switch {
				FilterOperator.EqualTo => s => s.DateOfBirth == dateOfBirth,
				FilterOperator.NotEqualTo => s => s.DateOfBirth != dateOfBirth,
				FilterOperator.LessThan => s => s.DateOfBirth < dateOfBirth,
				FilterOperator.LessThanOrEqualTo => s => s.DateOfBirth <= dateOfBirth,
				FilterOperator.GreaterThan => s => s.DateOfBirth > dateOfBirth,
				FilterOperator.GreaterThanOrEqualTo => s => s.DateOfBirth >= dateOfBirth,
				_ => throw new Exception( "Unsupported filter" ),
			};
		}
	}
}
