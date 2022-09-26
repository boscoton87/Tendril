using NUnit.Framework;
using Tendril.Services;
using Tendril.EFCore.Test.Mocks.Models;
using Tendril.Models;
using Tendril.Enums;
using Tendril.Exceptions;
using AutoMapper;
using Microsoft.Data.Sqlite;

namespace Tendril.EFCore.Test.Extensions {
	[TestFixture]
	public class EfCoreRegistrationExtensionsTests {
		public EfCoreRegistrationExtensionsTests() {
			DataManager = new DataManager();
		}

		private string ConnectionString { get; } = "Data Source=TestDb;Mode=Memory;Cache=Shared";

		private SqliteConnection? Connection { get; set; }

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
			Connection = new SqliteConnection( ConnectionString );
			Connection.Open();
			using var dbContext = new StudentDbContext( ConnectionString );
			dbContext.Database.EnsureCreated();
			DataManager = DataManagerBuilder.BuildDataManager( Mapper!, ConnectionString );
		}

		[TearDown]
		public void Cleanup() {
			Connection!.Close();
			using var dbContext = new StudentDbContext( ConnectionString );
			dbContext.Database.EnsureDeleted();
		}

		private async Task InsertStudentDtos( params string[] names ) {
			var students = names.Select( name => new StudentDto { Name = name, IsEnrolled = true, DateOfBirth = new DateTime( 1991, 6, 11 ) } ).ToList();
			if ( students.Count == 1 ) {
				await DataManager.Create( students.Single() );
			} else {
				await DataManager.CreateRange( students );
			}
		}

		private async Task InsertStudents( params string[] names ) {
			var students = names.Select( name => new Student { Name = name, IsEnrolled = true, DateOfBirth = new DateTime( 1991, 6, 11 ) } ).ToList();
			if ( students.Count == 1 ) {
				await DataManager.Create( students.Single() );
			} else {
				await DataManager.CreateRange( students );
			}
		}

		private async Task<Student> UpdateStudentWithNewName( int id, string name ) {
			var students = await DataManager.FindByFilter<Student>( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, id ) );
			var student = students.Single();
			student.Name = name;
			await DataManager.Update( student );
			return student;
		}

		private async Task<StudentDto> UpdateStudentDtoWithNewName( int id, string name ) {
			var students = await DataManager.FindByFilter<StudentDto>( new ValueFilterChip<StudentDto, int>( s => s.Id, FilterOperator.EqualTo, id ) );
			var student = students.Single();
			student.Name = name;
			await DataManager.Update( student );
			return student;
		}

		private async Task DeleteRecords<TModel>( FilterChip filter ) where TModel : class {
			foreach ( var record in await DataManager.FindByFilter<TModel>( filter ) ) {
				await DataManager.Delete( record );
			}
		}

		private void AssertStudentsInResult( IEnumerable<Student> students, params string[] names ) {
			var studentList = students.ToList();
			Assert.AreEqual( names.Length, studentList.Count );
			for ( int index = 0; index < names.Length; index++ ) {
				Assert.AreEqual( names[ index ], studentList[ index ].Name );
			}
		}

		private void AssertStudentsInResult( IEnumerable<StudentDto> students, params string[] names ) {
			var studentList = students.ToList();
			Assert.AreEqual( names.Length, studentList.Count );
			for ( int index = 0; index < names.Length; index++ ) {
				Assert.AreEqual( names[ index ], studentList[ index ].Name );
			}
		}

		[Test]
		public async Task TestCreate() {
			var names = new string[] { "John Doe" };
			await InsertStudents( names );
			AssertStudentsInResult( await DataManager.FindByFilter<Student>(), names );
		}

		[Test]
		public async Task TestCreateDto() {
			var names = new string[] { "John Doe" };
			await InsertStudentDtos( names );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>(), names );
		}

		[Test]
		public async Task TestCreateRange() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudents( names );
			AssertStudentsInResult( await DataManager.FindByFilter<Student>(), names );
		}

		[Test]
		public async Task TestCreateRangeDto() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>(), names );
		}

		[Test]
		public async Task TestUpdate() {
			var names = new string[] { "John Doe" };
			await InsertStudents( names );
			var newName = "Jon Doe";
			await UpdateStudentWithNewName( 1, newName );
			AssertStudentsInResult( await DataManager.FindByFilter<Student>(), newName );
		}

		[Test]
		public async Task TestUpdateDto() {
			var names = new string[] { "John Doe" };
			await InsertStudentDtos( names );
			var newName = "Jon Doe";
			await UpdateStudentDtoWithNewName( 1, newName );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>(), newName );
		}

		[Test]
		public async Task TestDelete() {
			var names = new string[] { "John Doe", "Jane Doe" };
			await InsertStudents( names );
			AssertStudentsInResult( await DataManager.FindByFilter<Student>(), names );
			await DeleteRecords<Student>( new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.EqualTo, names[ 0 ] ) );
			AssertStudentsInResult( await DataManager.FindByFilter<Student>(), names[ 1 ] );
		}

		[Test]
		public async Task TestDeleteDto() {
			var names = new string[] { "John Doe", "Jane Doe" };
			await InsertStudentDtos( names );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>(), names );
			await DeleteRecords<StudentDto>( new ValueFilterChip<StudentDto, string>( s => s.Name, FilterOperator.EqualTo, names[ 0 ] ) );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>(), names[ 1 ] );
		}

		[Test]
		public async Task TestSimpleFilter() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var filter = new ValueFilterChip<Student, string>( n => n.Name, FilterOperator.EqualTo, names[ 0 ] );
			AssertStudentsInResult( await DataManager.FindByFilter<Student>( filter ), names[ 0 ] );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>( filter ), names[ 0 ] );
		}

		[Test]
		public async Task TestAndFilters() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var filter = new AndFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ),
				new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "Ja" )
			);
			AssertStudentsInResult( await DataManager.FindByFilter<Student>( filter ), names[ 1 ] );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>( filter ), names[ 1 ] );
		}

		[Test]
		public async Task TestOrFilters() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var filter = new OrFilterChip(
					new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 ),
					new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "Ja" )
				);
			AssertStudentsInResult( await DataManager.FindByFilter<Student>( filter ), names[ 0 ], names[ 1 ], names[ 3 ] );
			AssertStudentsInResult( await DataManager.FindByFilter<StudentDto>( filter ), names[ 0 ], names[ 1 ], names[ 3 ] );
		}

		[Test]
		public async Task TestCountSimpleFilter() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var filter = new ValueFilterChip<Student, string>( n => n.Name, FilterOperator.EqualTo, names[ 0 ] );
			Assert.AreEqual( await DataManager.CountByFilter<Student>( filter ), 1 );
			Assert.AreEqual( await DataManager.CountByFilter<StudentDto>( filter ), 1 );
		}

		[Test]
		public async Task TestCountAndFilters() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var filter = new AndFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ),
				new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "Ja" )
			);
			Assert.AreEqual( await DataManager.CountByFilter<Student>( filter ), 1 );
			Assert.AreEqual( await DataManager.CountByFilter<StudentDto>( filter ), 1 );
		}

		[Test]
		public async Task TestCountOrFilters() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var filter = new OrFilterChip(
					new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 ),
					new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "Ja" )
				);
			Assert.AreEqual( await DataManager.CountByFilter<Student>( filter ), 3 );
			Assert.AreEqual( await DataManager.CountByFilter<StudentDto>( filter ), 3 );
		}

		[Test]
		public async Task TestExecuteRawQuery() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudents( names );
			var results = await DataManager.ExecuteRawQuery<Student>( "SELECT * FROM Students s WHERE s.Name = @Name", new SqliteParameter( "Name", "Jane Doe" ) );
			AssertStudentsInResult( results, names[ 1 ] );
		}

		[Test]
		public async Task TestExecuteRawQueryDto() {
			var names = new string[] { "John Doe", "Jane Doe", "John Smith", "Jane Smith" };
			await InsertStudentDtos( names );
			var results = await DataManager.ExecuteRawQuery<StudentDto>( "SELECT * FROM Students s WHERE s.Name = @Name", new SqliteParameter( "Name", "Jane Doe" ) );
			AssertStudentsInResult( results, names[ 1 ] );
		}

		[Test]
		public void TestUnsupportedFilter() {
			Assert.CatchAsync<UnsupportedFilterException>(
				() => DataManager!.FindByFilter<StudentDto>( new ValueFilterChip<StudentDto, string>( s => s.Name, FilterOperator.LessThan, "foo" ) )
			);
		}
	}
}
