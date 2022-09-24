using NUnit.Framework;
using Tendril.Enums;
using Tendril.Models;
using Tendril.Services;
using Tendril.Test.Mocks.Models;

namespace Tendril.Test.Services {
	[TestFixture]
	public class LinqFindByFilterServiceTests {
		private LinqFindByFilterService<Student> _findByFilterService;

		private IQueryable<Student> _dataSet;

		[SetUp]
		public void Initialize() {
			_findByFilterService = new LinqFindByFilterService<Student>()
				.WithFilterType( s => s.Id, FilterOperator.EqualTo, v => s => s.Id == v.First() )
				.WithFilterType( s => s.Id, FilterOperator.NotEqualTo, v => s => s.Id != v.First() )
				.WithFilterType( s => s.Id, FilterOperator.In, v => s => v.Contains( s.Id ), minValueCount: 1, maxValueCount: 4 )
				.WithFilterType( s => s.Id, FilterOperator.NotIn, v => s => !v.Contains( s.Id ), minValueCount: 1, maxValueCount: 4 )
				.WithFilterType( s => s.Name, FilterOperator.EqualTo, v => s => s.Name == v.First() )
				.WithFilterType( s => s.Name, FilterOperator.NotEqualTo, v => s => s.Name != v.First() )
				.WithFilterType( s => s.Name, FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.First() ) )
				.WithFilterType( s => s.Name, FilterOperator.NotStartsWith, v => s => !s.Name.StartsWith( v.First() ) )
				.WithFilterType( s => s.Name, FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.First() ) )
				.WithFilterType( s => s.Name, FilterOperator.NotEndsWith, v => s => !s.Name.EndsWith( v.First() ) )
				.WithFilterType( s => s.IsEnrolled, FilterOperator.EqualTo, _ => s => s.IsEnrolled )
				.WithFilterType<bool>( "IsEnrolled", FilterOperator.NotEqualTo, _ => s => !s.IsEnrolled );

			RegisterListOfStudents( "John Doe", "Jane Doe", "Mike Smith", "Michelle Smith" );
		}

		private void RegisterListOfStudents( params string[] names ) {
			_dataSet = names.Select(
				( n, i ) => new Student {
					Id = i + 1,
					Name = n,
					IsEnrolled = true,
					DateOfBirth = new DateTime( 1500 )
				}
			).AsQueryable();
		}

		private List<Student> FindByFilter( FilterChip filter, int? page = null, int? pageSize = null ) {
			return _findByFilterService.FindByFilter( _dataSet, filter, page, pageSize ).ToList();
		}

		private void AssertResultFails( FilterChip filter, string message ) {
			var result = _findByFilterService.ValidateFilters( filter );
			Assert.IsFalse( result.IsSuccess );
			Assert.AreEqual( result.Message, message );
		}

		private void AssertResultPasses( FilterChip filter ) {
			var result = _findByFilterService.ValidateFilters( filter );
			Assert.IsTrue( result.IsSuccess );
			Assert.IsEmpty( result.Message );
		}

		[Test]
		public void TestSimpleFilter() {
			var results = FindByFilter( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 2 ) );
			Assert.AreEqual( 1, results.Count );
			var result = results.Single();
			Assert.AreEqual( 2, result.Id );
			Assert.AreEqual( "Jane Doe", result.Name );
		}

		[Test]
		public void TestAndFilters() {
			var results = FindByFilter(
				new AndFilterChip(
					new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ),
					new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "M" )
				)
			);
			Assert.AreEqual( 1, results.Count );
			var result = results.Single();
			Assert.AreEqual( 3, result.Id );
			Assert.AreEqual( "Mike Smith", result.Name );
		}

		[Test]
		public void TestOrFilters() {
			var results = FindByFilter(
				new OrFilterChip(
					new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 ),
					new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "Mich" )
				)
			);
			Assert.AreEqual( 2, results.Count );
			Assert.AreEqual( 1, results[ 0 ].Id );
			Assert.AreEqual( "John Doe", results[ 0 ].Name );
			Assert.AreEqual( 4, results[ 1 ].Id );
			Assert.AreEqual( "Michelle Smith", results[ 1 ].Name );
		}

		[Test]
		public void TestFilterPageOnlyNoPagination() {
			var results = FindByFilter( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ), 1 );
			Assert.AreEqual( 3, results.Count );
		}

		[Test]
		public void TestFilterPageSizeOnlyNoPagination() {
			var results = FindByFilter( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ), null, 2 );
			Assert.AreEqual( 3, results.Count );
		}

		[Test]
		public void TestFilterPagination() {
			var results = FindByFilter( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ), 0, 2 );
			Assert.AreEqual( 2, results.Count );
			Assert.AreEqual( 1, results[ 0 ].Id );
			Assert.AreEqual( "John Doe", results[ 0 ].Name );
			Assert.AreEqual( 2, results[ 1 ].Id );
			Assert.AreEqual( "Jane Doe", results[ 1 ].Name );
		}

		[Test]
		public void TestFilterPaginationPartialPage() {
			var results = FindByFilter( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ), 1, 2 );
			Assert.AreEqual( 1, results.Count );
			var result = results.Single();
			Assert.AreEqual( 3, result.Id );
			Assert.AreEqual( "Mike Smith", result.Name );
		}

		[Test]
		public void InvalidOrFilterFails() {
			AssertResultFails( new OrFilterChip(), "OrFilterChip values must contain at least 1 FilterChip" );
		}

		[Test]
		public void OrFilterWithNullValueFails() {
			var filters = new OrFilterChip( new FilterChip[] { null } );
			AssertResultFails( filters, "OrFilterChip values must contain at least 1 FilterChip" );
		}

		[Test]
		public void ValidOrFilterPasses() {
			var filters = new OrFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void InvalidAndFilterFails() {
			AssertResultFails( new AndFilterChip(), "AndFilterChip values must contain at least 1 FilterChip" );
		}

		[Test]
		public void AndFilterWithNullValueFails() {
			var filters = new AndFilterChip( new FilterChip[] { null } );
			AssertResultFails( filters, "AndFilterChip values must contain at least 1 FilterChip" );
		}

		[Test]
		public void ValidAndFilterPasses() {
			var filters = new AndFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void WithMaxFilterDepthTooLowThrows() {
			Assert.Throws<ArgumentException>(
				() => _findByFilterService.WithMaxFilterDepth( 0 ),
				"maxFilterDepth must be greater than or equal to 1"
			);
		}

		[Test]
		public void ExceededMaxDepthFails() {
			var filters = new OrFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 )
			);
			_findByFilterService.WithMaxFilterDepth( 1 );
			AssertResultFails( filters, "Filter with depth of 2 found, max supported depth is 1" );
		}

		[Test]
		public void WithinMaxDepthPasses() {
			var filters = new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 );
			_findByFilterService.WithMaxFilterDepth( 1 );
			AssertResultPasses( filters );
		}

		[Test]
		public void NoMaxDepthPasses() {
			var filters = new OrFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void AllowNullFilterPasses() {
			AssertResultPasses( null );
		}

		[Test]
		public void RejectNullFilterFails() {
			_findByFilterService.RejectNullFilter();
			AssertResultFails( null, "Filter must not be null" );
		}

		[Test]
		public void RejectUndefinedFiltersFails() {
			var filters = new ValueFilterChip<Student, string>( "foo", FilterOperator.EqualTo, "bar" );
			AssertResultFails( filters, "Undefined filter provided" );
		}

		[Test]
		public void RejectDuplicateFieldsFails() {
			var filters = new AndFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ),
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.NotIn, 4, 5, 6 )
			);
			_findByFilterService.HasDistinctFields();
			AssertResultFails( filters, "Duplicate filter provided" );
		}

		[Test]
		public void AllowDuplicateFieldsFails() {
			var filters = new AndFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3 ),
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.NotIn, 4, 5, 6 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void MissingRequiredFieldFails() {
			var filters = new OrFilterChip(
				new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 1 )
			);
			_findByFilterService.WithFilterType( s => s.Name, FilterOperator.Contains, v => s => s.Name.Contains( v.First() ), true );
			AssertResultFails( filters, "Name filter was not provided" );
		}

		[Test]
		public void NullFilterValuesFails() {
			AssertResultFails( new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.EqualTo, null ), "Name filter values must not be null" );
		}

		[Test]
		public void TooFewFilterValuesFails() {
			AssertResultFails( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In ), "Id filter values length must be greater than 0 and less than 5" );
		}

		[Test]
		public void TooManyFilterValuesFails() {
			AssertResultFails( new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.In, 1, 2, 3, 4, 5 ), "Id filter values length must be greater than 0 and less than 5" );
		}

		[Test]
		public void HasFilterTypeMinValueCountTooLowThrows() {
			Assert.Throws<ArgumentException>(
				() => _findByFilterService.WithFilterType( s => s.Id, FilterOperator.In, v => s => v.Contains( s.Id ), false, -1, 0 ),
				"minValueCount must be greater than or equal to "
			);
		}

		[Test]
		public void HasFilterTypeMinValueCountLessThanMaxValueCountThrows() {
			Assert.Throws<ArgumentException>(
				() => _findByFilterService.WithFilterType( s => s.Id, FilterOperator.In, v => s => v.Contains( s.Id ), false, 1, 0 ),
				"maxValueCount must be greater than or equal to minValueCount"
			);
		}

		[Test]
		public void InvalidFilterValueTypeFails() {
			var filters = new ValueFilterChip<Student, float>( "Id", FilterOperator.EqualTo, 1.1f );
			AssertResultFails( filters, "Id filter values must be of type Int32" );
		}
	}
}
