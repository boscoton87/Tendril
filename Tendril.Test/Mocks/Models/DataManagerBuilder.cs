using Tendril.Enums;
using Tendril.InMemory.Extensions;
using Tendril.InMemory.Services;
using Tendril.Services;

namespace Tendril.Test.Mocks.Models {
	internal static class DataManagerBuilder {
		public static DataManager BuildDataManager() {
			var findByFilterService = new LinqFindByFilterService<Student>()
				.WithFilterDefinition( s => s.Id, FilterOperator.EqualTo, v => s => s.Id == v.First() )
				.WithFilterDefinition( s => s.Id, FilterOperator.NotEqualTo, v => s => s.Id != v.First() )
				.WithFilterDefinition( s => s.Id, FilterOperator.In, v => s => v.Select( v => v ).Contains( s.Id ) )
				.WithFilterDefinition( s => s.Id, FilterOperator.NotIn, v => s => !v.Select( v => v ).Contains( s.Id ) )
				.WithFilterDefinition( s => s.Name, FilterOperator.EqualTo, v => s => s.Name == v.First() )
				.WithFilterDefinition( s => s.Name, FilterOperator.NotEqualTo, v => s => s.Name != v.First() )
				.WithFilterDefinition( s => s.Name, FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.First() ) )
				.WithFilterDefinition( s => s.Name, FilterOperator.NotStartsWith, v => s => !s.Name.StartsWith( v.First() ) )
				.WithFilterDefinition( s => s.Name, FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.First() ) )
				.WithFilterDefinition( s => s.Name, FilterOperator.NotEndsWith, v => s => !s.Name.EndsWith( v.First() ) )
				.WithFilterDefinition( s => s.IsEnrolled, FilterOperator.EqualTo, v => s => s.IsEnrolled == v.First() )
				.WithFilterDefinition( s => s.IsEnrolled, FilterOperator.NotEqualTo, v => s => s.IsEnrolled != v.First() )
				.WithFilterDefinition( s => s.DateOfBirth, FilterOperator.EqualTo, v => s => s.DateOfBirth == v.First() )
				.WithFilterDefinition( s => s.DateOfBirth, FilterOperator.NotEqualTo, v => s => s.DateOfBirth != v.First() )
				.WithFilterDefinition( s => s.DateOfBirth, FilterOperator.LessThan, v => s => s.DateOfBirth < v.First() )
				.WithFilterDefinition( s => s.DateOfBirth, FilterOperator.LessThanOrEqualTo, v => s => s.DateOfBirth <= v.First() )
				.WithFilterDefinition( s => s.DateOfBirth, FilterOperator.GreaterThan, v => s => s.DateOfBirth > v.First() )
				.WithFilterDefinition( s => s.DateOfBirth, FilterOperator.GreaterThanOrEqualTo, v => s => s.DateOfBirth >= v.First() );
			var filterChipValidator = new FilterChipValidatorService<Student>()
				.HasDistinctFields()
				.HasFilterType( s => s.Id, false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
				.HasFilterType( s => s.Id, false, 1, 10, FilterOperator.In, FilterOperator.NotIn )
				.HasFilterType(
					s => s.Name, false, 1, 1,
					FilterOperator.EqualTo, FilterOperator.NotEqualTo, FilterOperator.StartsWith,
					FilterOperator.NotStartsWith, FilterOperator.EndsWith, FilterOperator.NotEndsWith
				)
				.HasFilterType( s => s.IsEnrolled, false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
				.HasFilterType(
					s => s.DateOfBirth, false, 1, 1,
					FilterOperator.EqualTo, FilterOperator.NotEqualTo, FilterOperator.LessThan, FilterOperator.LessThanOrEqualTo,
					FilterOperator.GreaterThan, FilterOperator.GreaterThanOrEqualTo
				);
			var dataManager = new DataManager();
			dataManager
				.WithInMemoryCache()
					.WithDbSet(
						s => s.Id,
						( k, s ) => s.Id = k,
						new KeyGeneratorSequential<Student, int>( ( k, _ ) => k + 1 ),
						findByFilterService,
						filterChipValidator
					);
			return dataManager;
		}
	}
}
