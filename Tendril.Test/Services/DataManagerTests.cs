using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;
using Tendril.Enums;
using Tendril.Exceptions;
using Tendril.InMemory.Test.Mocks.Models;
using Tendril.Models;
using Tendril.Services;
using Tendril.Test.Mocks.Models;
using Tendril.Test.Mocks.Services;

namespace Tendril.Test.Services {
	public class DataManagerTests {
		private DataManager DataManager { get; set; }
		private MockDataCollection DataCollection { get; set; }
		private MockDataCollection DataCollectionMapped { get; set; }
		private IMapper Mapper { get; set; }

		[OneTimeSetUp]
		public void PreTestInitialize() {
			var mapperConfig = new MapperConfiguration( cfg => {
				cfg.CreateMap<StudentDto, Student>();
				cfg.CreateMap<Student, StudentDto>();
			} );
			mapperConfig.CompileMappings();
			Mapper = mapperConfig.CreateMapper();
		}

		[SetUp]
		public void Initialize() {
			DataCollection = new MockDataCollection();
			DataCollectionMapped = new MockDataCollection();
			DataManager = new DataManager()
				.WithDataCollection( DataCollection )
				.WithDataCollection( DataCollectionMapped, Mapper.Map<StudentDto, Student>, Mapper.Map<Student, StudentDto> );
		}

		private Student MakeStudent( string name ) {
			return new Student {
				Id = 2,
				Name = name,
				IsEnrolled = true,
				DateOfBirth = new DateTime( 1200 )
			};
		}

		private StudentDto MakeStudentDto( string name ) {
			return new StudentDto {
				Id = 2,
				Name = name,
				IsEnrolled = true,
				DateOfBirth = new DateTime( 1200 )
			};
		}

		[Test]
		public async Task TestCreate() {
			var student = MakeStudent( "John" );
			await DataManager.Create( student );
			DataCollection.AssertCallMade( 0, 1, "Add", student );
		}

		[Test]
		public async Task TestCreateMapped() {
			var student = MakeStudentDto( "John" );
			await DataManager.Create( student );
			DataCollectionMapped.AssertCallMade( 0, 1, "Add", Mapper.Map<StudentDto, Student>( student ) );
		}

		[Test]
		public async Task TestCreateRange() {
			var students = new List<Student> {
				MakeStudent( "John" ),
				MakeStudent( "Jane" ),
			};
			await DataManager.CreateRange( students );
			DataCollection.AssertCallMade( 0, 1, "AddRange", students );
		}

		[Test]
		public async Task TestCreateRangeBatched() {
			var studentsA = new List<Student> { MakeStudent( "John" ) };
			var studentsB = new List<Student> { MakeStudent( "Jane" ) };
			await DataManager.CreateRange( studentsA.Concat( studentsB ), 1 );
			DataCollection.AssertCallMade( 0, 2, "AddRange", studentsA );
			DataCollection.AssertCallMade( 1, 2, "AddRange", studentsB );
		}

		[Test]
		public async Task TestCreateRangeMapped() {
			var students = new List<StudentDto> {
				MakeStudentDto( "John" ),
				MakeStudentDto( "Jane" ),
			};
			await DataManager.CreateRange( students );
			DataCollectionMapped.AssertCallMade( 0, 1, "AddRange", students.Select( s => Mapper.Map<StudentDto, Student>( s ) ) );
		}

		[Test]
		public async Task TestUpdate() {
			var student = MakeStudent( "John" );
			await DataManager.Update( student );
			DataCollection.AssertCallMade( 0, 1, "Update", student );
		}

		[Test]
		public async Task TestUpdateMapped() {
			var student = MakeStudentDto( "John" );
			await DataManager.Update( student );
			DataCollectionMapped.AssertCallMade( 0, 1, "Update", Mapper.Map<StudentDto, Student>( student ) );
		}

		[Test]
		public async Task TestDelete() {
			var student = MakeStudent( "John" );
			await DataManager.Delete( student );
			DataCollection.AssertCallMade( 0, 1, "Delete", student );
		}

		[Test]
		public async Task TestDeleteDto() {
			var student = MakeStudentDto( "John" );
			await DataManager.Delete( student );
			DataCollectionMapped.AssertCallMade( 0, 1, "Delete", Mapper.Map<StudentDto, Student>( student ) );
		}

		[Test]
		public void TestFindByFilterUnsupportedFilterThrows() {
			var filter = new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 );
			Assert.ThrowsAsync<UnsupportedFilterException>(
				async () => await DataManager.FindByFilter<Student>( filter )
			);
			DataCollection.AssertCallMade( 0, 1, "ValidateFilters", filter );
		}

		[Test]
		public async Task TestFindByFilter() {
			var filter = new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.EqualTo, "John" );
			await DataManager.FindByFilter<Student>( filter, 1, 2 );
			DataCollection.AssertCallMade( 0, 2, "ValidateFilters", filter );
			DataCollection.AssertCallMade( 1, 2, "FindByFilter", filter, 1, 2 );
		}

		[Test]
		public async Task TestFindByFilterDto() {
			var filter = new ValueFilterChip<StudentDto, string>( s => s.Name, FilterOperator.EqualTo, "John" );
			await DataManager.FindByFilter<StudentDto>( filter, 1, 2 );
			DataCollectionMapped.AssertCallMade( 0, 2, "ValidateFilters", filter );
			DataCollectionMapped.AssertCallMade( 1, 2, "FindByFilter", filter, 1, 2 );
		}

		[Test]
		public async Task TestCountByFilter() {
			var filter = new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.EqualTo, "John" );
			await DataManager.CountByFilter<Student>( filter, 1, 2 );
			DataCollection.AssertCallMade( 0, 2, "ValidateFilters", filter );
			DataCollection.AssertCallMade( 1, 2, "CountByFilter", filter, 1, 2 );
		}

		[Test]
		public async Task TestCountByFilterDto() {
			var filter = new ValueFilterChip<StudentDto, string>( s => s.Name, FilterOperator.EqualTo, "John" );
			await DataManager.CountByFilter<StudentDto>( filter, 1, 2 );
			DataCollectionMapped.AssertCallMade( 0, 2, "ValidateFilters", filter );
			DataCollectionMapped.AssertCallMade( 1, 2, "CountByFilter", filter, 1, 2 );
		}

		[Test]
		public async Task TestExecuteRawQuery() {
			await DataManager.ExecuteRawQuery<Student>( "SOME SQL QUERY", "foo" );
			DataCollection.AssertCallMade( 0, 1, "ExecuteRawQuery", "SOME SQL QUERY", new object[] { "foo" } );
		}

		[Test]
		public async Task TestExecuteRawQueryDto() {
			await DataManager.ExecuteRawQuery<StudentDto>( "SOME SQL QUERY", "foo" );
			DataCollectionMapped.AssertCallMade( 0, 1, "ExecuteRawQuery", "SOME SQL QUERY", new object[] { "foo" } );
		}
	}
}
