using AutoMapper;
using Tendril.Enums;
using Tendril.InMemory.Extensions;
using Tendril.InMemory.Services;
using Tendril.Services;

namespace Tendril.InMemory.Test.Mocks.Models {
	internal static class DataManagerBuilder {
		public static DataManager BuildDataManager( IMapper mapper ) {
			var findByFilterService = new LinqFindByFilterService<Student>()
				.HasDistinctFields()
				.WithFilterType( s => s.Id, FilterOperator.EqualTo, v => s => s.Id == v.First() )
				.WithFilterType( s => s.Id, FilterOperator.NotEqualTo, v => s => s.Id != v.First() )
				.WithFilterType( s => s.Id, FilterOperator.In, v => s => v.Select( v => v ).Contains( s.Id ), maxValueCount: 10 )
				.WithFilterType( s => s.Id, FilterOperator.NotIn, v => s => !v.Select( v => v ).Contains( s.Id ), maxValueCount: 10 )
				.WithFilterType( s => s.Name, FilterOperator.EqualTo, v => s => s.Name == v.First() )
				.WithFilterType( s => s.Name, FilterOperator.NotEqualTo, v => s => s.Name != v.First() )
				.WithFilterType( s => s.Name, FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.First() ) )
				.WithFilterType( s => s.Name, FilterOperator.NotStartsWith, v => s => !s.Name.StartsWith( v.First() ) )
				.WithFilterType( s => s.Name, FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.First() ) )
				.WithFilterType( s => s.Name, FilterOperator.NotEndsWith, v => s => !s.Name.EndsWith( v.First() ) )
				.WithFilterType( s => s.IsEnrolled, FilterOperator.EqualTo, v => s => s.IsEnrolled == v.First() )
				.WithFilterType( s => s.IsEnrolled, FilterOperator.NotEqualTo, v => s => s.IsEnrolled != v.First() )
				.WithFilterType( s => s.DateOfBirth, FilterOperator.EqualTo, v => s => s.DateOfBirth == v.First() )
				.WithFilterType( s => s.DateOfBirth, FilterOperator.NotEqualTo, v => s => s.DateOfBirth != v.First() )
				.WithFilterType( s => s.DateOfBirth, FilterOperator.LessThan, v => s => s.DateOfBirth < v.First() )
				.WithFilterType( s => s.DateOfBirth, FilterOperator.LessThanOrEqualTo, v => s => s.DateOfBirth <= v.First() )
				.WithFilterType( s => s.DateOfBirth, FilterOperator.GreaterThan, v => s => s.DateOfBirth > v.First() )
				.WithFilterType( s => s.DateOfBirth, FilterOperator.GreaterThanOrEqualTo, v => s => s.DateOfBirth >= v.First() );
			var dataManager = new DataManager();
			dataManager
				.WithInMemoryCache()
					.WithCacheSet(
						new KeyGeneratorSequential<Student, int>(
							( k, _ ) => k + 1,
							s => s.Id,
							( k, s ) => s.Id = k
						),
						findByFilterService,
						mapper.Map<StudentDto, Student>,
						mapper.Map<Student, StudentDto>
					)
					.WithCacheSet(
						new KeyGeneratorSequential<Student, int>(
							( k, _ ) => k + 1,
							s => s.Id,
							( k, s ) => s.Id = k
						),
						findByFilterService
					);
			return dataManager;
		}
	}
}
