using Tendril.Enums;
using Tendril.InMemory.Extensions;
using Tendril.InMemory.Services;
using Tendril.Services;

namespace Tendril.Test.Mocks.Models {
	internal static class DataManagerBuilder {
		public static DataManager BuildDataManager() {
			var findByFilterService = new LinqFindByFilterService<Student>()
				.WithFilterDefinition( "Id", FilterOperator.EqualTo, v => s => s.Id == ( ( int ) v.First() ) )
				.WithFilterDefinition( "Id", FilterOperator.NotEqualTo, v => s => s.Id != ( ( int ) v.First() ) )
				.WithFilterDefinition( "Id", FilterOperator.In, v => s => v.Select( v => ( int ) v ).Contains( s.Id ) )
				.WithFilterDefinition( "Id", FilterOperator.NotIn, v => s => !v.Select( v => ( int ) v ).Contains( s.Id ) )
				.WithFilterDefinition( "Name", FilterOperator.EqualTo, v => s => s.Name == ( v.Single() as string ) )
				.WithFilterDefinition( "Name", FilterOperator.NotEqualTo, v => s => s.Name != ( v.Single() as string ) )
				.WithFilterDefinition( "Name", FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.Single() as string ) )
				.WithFilterDefinition( "Name", FilterOperator.NotStartsWith, v => s => !s.Name.StartsWith( v.Single() as string ) )
				.WithFilterDefinition( "Name", FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.Single() as string ) )
				.WithFilterDefinition( "Name", FilterOperator.NotEndsWith, v => s => !s.Name.EndsWith( v.Single() as string ) )
				.WithFilterDefinition( "IsEnrolled", FilterOperator.EqualTo, v => s => s.IsEnrolled == ( bool ) v.Single() )
				.WithFilterDefinition( "IsEnrolled", FilterOperator.NotEqualTo, v => s => s.IsEnrolled != ( bool ) v.Single() )
				.WithFilterDefinition( "DateOfBirth", FilterOperator.EqualTo, v => s => s.DateOfBirth == ( DateTime ) v.Single() )
				.WithFilterDefinition( "DateOfBirth", FilterOperator.NotEqualTo, v => s => s.DateOfBirth != ( DateTime ) v.Single() )
				.WithFilterDefinition( "DateOfBirth", FilterOperator.LessThan, v => s => s.DateOfBirth < ( DateTime ) v.Single() )
				.WithFilterDefinition( "DateOfBirth", FilterOperator.LessThanOrEqualTo, v => s => s.DateOfBirth <= ( DateTime ) v.Single() )
				.WithFilterDefinition( "DateOfBirth", FilterOperator.GreaterThan, v => s => s.DateOfBirth > ( DateTime ) v.Single() )
				.WithFilterDefinition( "DateOfBirth", FilterOperator.GreaterThanOrEqualTo, v => s => s.DateOfBirth >= ( DateTime ) v.Single() );
			var dataManager = new DataManager();
			dataManager
				.WithInMemoryCache()
					.WithDbSet(
						s => s.Id,
						( k, s ) => s.Id = k,
						new KeyGeneratorSequential<Student, int>( ( k, _ ) => k + 1 ),
						new FilterChipValidatorService()
							.HasDistinctFields()
							.HasFilterType<int>( "Id", false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
							.HasFilterType<int>( "Id", false, 1, 10, FilterOperator.In, FilterOperator.NotIn )
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
						( cache, filter, page, pageSize ) => Task.FromResult(
							findByFilterService.FindByFilter(
								cache.Values.Select( v => v as Student ).AsQueryable(),
								filter,
								page,
								pageSize
							).AsEnumerable()
						)
					);
			return dataManager;
		}
	}
}
