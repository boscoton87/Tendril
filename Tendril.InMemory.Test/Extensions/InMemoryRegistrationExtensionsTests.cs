using NUnit.Framework;
using Tendril.Services;
using Tendril.InMemory.Test.Mocks.Models;
using Tendril.Models;
using Tendril.Enums;
using Tendril.Exceptions;
using AutoMapper;

namespace Tendril.InMemory.Test.Extensions {
	[TestFixture]
	public class InMemoryRegistrationExtensionsTests {
		public InMemoryRegistrationExtensionsTests() {
			DataManager = new DataManager();
		}

		private DataManager DataManager { get; set; }

		private IMapper? Mapper { get; set; }

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
			DataManager = DataManagerBuilder.BuildDataManager( Mapper! );
		}

		private async Task InsertStudents( params string[] names ) {
			var students = names.Select( name => new StudentDto { Name = name, IsEnrolled = true, DateOfBirth = new DateTime( 1991, 6, 11 ) } );
			await DataManager!.CreateRange( students, 10 );
		}

		[Test]
		public async Task TestInsertStudent() {
			await InsertStudents( "John Doe" );
			var students = await DataManager!.FindByFilter<StudentDto>();
			var student = students.Single();
			Assert.AreEqual( student.Id, 1 );
			Assert.AreEqual( student.Name, "John Doe" );
			Assert.IsTrue( student.IsEnrolled );
			Assert.AreEqual( student.DateOfBirth, new DateTime( 1991, 6, 11 ) );
		}

		[Test]
		public async Task TestFilterStudents() {
			await InsertStudents( "John Doe", "Jane Doe", "John Smith", "Jane Smith" );
			var students = await DataManager!.FindByFilter<StudentDto>( new ValueFilterChip<StudentDto, string>( s => s.Name, FilterOperator.StartsWith, "Jane" ) );
			Assert.AreEqual( 2, students.Count() );
			Assert.IsTrue( students.Any( s => s.Name == "Jane Doe" ) );
			Assert.IsTrue( students.Any( s => s.Name == "Jane Smith" ) );
		}

		[Test]
		public void TestUnsupportedFilter() {
			Assert.CatchAsync<UnsupportedFilterException>(
				() => DataManager!.FindByFilter<StudentDto>( new ValueFilterChip<StudentDto, string>( s => s.Name, FilterOperator.LessThan, "foo" ) )
			);
		}
	}
}
