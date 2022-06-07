using NUnit.Framework;
using Tendril.Enums;
using Tendril.Models;

namespace Tendril.Test.Models {
	[TestFixture]
	public class FilterChipValidatorTests {
		private FilterChipValidator _validator;

		[SetUp]
		public void Initialize() {
			_validator = new FilterChipValidator()
				.HasFilterType<int>( "Id", false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo )
				.HasFilterType<int>( "Id", false, 2, 3, FilterOperator.In, FilterOperator.NotIn )
				.HasFilterType<string>(
					"Name", false, 1, 1,
					FilterOperator.EqualTo, FilterOperator.NotEqualTo, FilterOperator.StartsWith,
					FilterOperator.NotStartsWith, FilterOperator.EndsWith, FilterOperator.NotEndsWith
				)
				.HasFilterType<bool>( "IsEnrolled", false, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo );
		}

		private void AssertResultFails( FilterChip filter, string message ) {
			var result = _validator.ValidateFilters( filter );
			Assert.IsFalse( result.IsSuccess );
			Assert.AreEqual( result.Message, message );
		}

		private void AssertResultPasses( FilterChip filter ) {
			var result = _validator.ValidateFilters( filter );
			Assert.IsTrue( result.IsSuccess );
			Assert.IsEmpty( result.Message );
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
				new FilterChip( "Id", FilterOperator.EqualTo, 1 )
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
				new FilterChip( "Id", FilterOperator.EqualTo, 1 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void WithMaxFilterDepthTooLowThrows() {
			Assert.Throws<ArgumentException>(
				() => _validator.WithMaxFilterDepth( 0 ),
				"maxFilterDepth must be greater than or equal to 1"
			);
		}

		[Test]
		public void ExceededMaxDepthFails() {
			var filters = new OrFilterChip(
				new FilterChip( "Id", FilterOperator.EqualTo, 1 )
			);
			_validator.WithMaxFilterDepth( 1 );
			AssertResultFails( filters, "Filter with depth of 2 found, max supported depth is 1" );
		}

		[Test]
		public void WithinMaxDepthPasses() {
			var filters = new FilterChip( "Id", FilterOperator.EqualTo, 1 );
			_validator.WithMaxFilterDepth( 1 );
			AssertResultPasses( filters );
		}

		[Test]
		public void NoMaxDepthPasses() {
			var filters = new OrFilterChip(
				new FilterChip( "Id", FilterOperator.EqualTo, 1 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void AllowNullFilterPasses() {
			AssertResultPasses( null );
		}

		[Test]
		public void RejectNullFilterFails() {
			_validator.RejectNullFilter();
			AssertResultFails( null, "Filter must not be null" );
		}

		[Test]
		public void RejectUndefinedFiltersFails() {
			var filters = new FilterChip( "foo", FilterOperator.EqualTo, "bar" );
			AssertResultFails( filters, "Undefined filter provided" );
		}

		[Test]
		public void AllowUndefinedFiltersPasses() {
			var filters = new FilterChip( "foo", FilterOperator.EqualTo, "bar" );
			_validator.AllowUndefinedFilters();
			AssertResultPasses( filters );
		}

		[Test]
		public void RejectDuplicateFieldsFails() {
			var filters = new AndFilterChip(
				new FilterChip( "Id", FilterOperator.In, 1, 2, 3 ),
				new FilterChip( "Id", FilterOperator.NotIn, 4, 5, 6 )
			);
			_validator.HasDistinctFields();
			AssertResultFails( filters, "Duplicate filter provided" );
		}

		[Test]
		public void AllowDuplicateFieldsFails() {
			var filters = new AndFilterChip(
				new FilterChip( "Id", FilterOperator.In, 1, 2, 3 ),
				new FilterChip( "Id", FilterOperator.NotIn, 4, 5, 6 )
			);
			AssertResultPasses( filters );
		}

		[Test]
		public void MissingRequiredFieldFails() {
			var filters = new OrFilterChip(
				new FilterChip( "Id", FilterOperator.EqualTo, 1 )
			);
			_validator.HasFilterType<string>( "Degree", true, 1, 1, FilterOperator.EqualTo, FilterOperator.NotEqualTo );
			AssertResultFails( filters, "Degree filter was not provided" );
		}

		[Test]
		public void NullFilterValuesFails() {
			AssertResultFails( new FilterChip( "Name", FilterOperator.EqualTo, null ), "Name filter values must not be null" );
		}

		[Test]
		public void TooFewFilterValuesFails() {
			AssertResultFails( new FilterChip( "Id", FilterOperator.In, 1 ), "Id filter values length must be greater than 1 and less than 4" );
		}

		[Test]
		public void TooManyFilterValuesFails() {
			AssertResultFails( new FilterChip( "Id", FilterOperator.In, 1, 2, 3, 4 ), "Id filter values length must be greater than 1 and less than 4" );
		}

		[Test]
		public void HasFilterTypeMinValueCountTooLowThrows() {
			Assert.Throws<ArgumentException>(
				() => _validator.HasFilterType<int>( "foo", false, 0, 1, FilterOperator.In ),
				"minValueCount must be greater than or equal to 1"
			);
		}

		[Test]
		public void InvalidFilterValueTypeFails() {
			var filters = new FilterChip( "Id", FilterOperator.EqualTo, 1.1 );
			AssertResultFails( filters, "Id filter values must be of type Int32" );
		}
	}
}
