using Tendril.Enums;

namespace Tendril.Models {
	/// <summary>
	/// Model class that defines a filter for a given query
	/// <b>Example:</b><br />
	/// // Find all records where <b>AgeInDays</b> is greater than or equal to 10<br />
	/// var filter = new FilterChip( "AgeInDays", FilterOperator.GreaterThanOrEqualTo, 10 );
	/// </summary>
	public class FilterChip {
		public string Field { get; }

		public FilterOperator? Operator { get; }

		public object[] Values { get; }

		public FilterChip( string field, FilterOperator? filterOperator, params object[] values ) {
			Field = field;
			Operator = filterOperator;
			Values = values;
		}
	}
}
