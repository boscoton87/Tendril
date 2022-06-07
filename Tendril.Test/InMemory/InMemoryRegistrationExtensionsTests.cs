using NUnit.Framework;
using Tendril.Services;
using Tendril.Test.Mocks.Models;
using Tendril.Models;
using Tendril.Enums;
using Tendril.Exceptions;

namespace Tendril.Test.InMemory {
	[TestFixture]
	public class InMemoryRegistrationExtensionsTests {
		private DataManager DataManager { get; set; }

		[SetUp]
		public void Initialize() {
			DataManager = DataManagerBuilder.BuildDataManager();
		}

		private async Task InsertStudents( params string[] names ) {
			var students = names.Select( name => new Student { Name = name, IsEnrolled = true, DateOfBirth = new DateTime( 1991, 6, 11 ) } );
			await DataManager.CreateRange( students, 10 );
		}

		[Test]
		public async Task TestInsertStudent() {
			await InsertStudents( "John Doe" );
			var students = await DataManager.FindByFilter<Student>();
			var student = students.Single();
			Assert.AreEqual( student.Id, 1 );
			Assert.AreEqual( student.Name, "John Doe" );
			Assert.IsTrue( student.IsEnrolled );
			Assert.AreEqual( student.DateOfBirth, new DateTime( 1991, 6, 11 ) );
		}

		[Test]
		public async Task TestFilterStudents() {
			await InsertStudents( "John Doe", "Jane Doe", "John Smith", "Jane Smith" );
			var students = await DataManager.FindByFilter<Student>( new FilterChip( "Name", FilterOperator.StartsWith, "Jane" ) );
			Assert.AreEqual( 2, students.Count() );
			Assert.IsTrue( students.Any( s => s.Name == "Jane Doe" ) );
			Assert.IsTrue( students.Any( s => s.Name == "Jane Smith" ) );
		}

		[Test]
		public void TestUnsupportedFilter() {
			Assert.CatchAsync<UnsupportedFilterException>(
				() => DataManager.FindByFilter<Student>(  new FilterChip( "Name", FilterOperator.LessThan, "foo" ) )
			);
		}
	}
}
