using Tendril.Enums;

namespace Tendril.Models {
	/// <summary>
	/// Model class that defines a filter for a given query
	/// <b>Example:</b><br />
	/// // Find all records where <b>AgeInDays</b> is greater than or equal to 10<br />
	/// var filter = new FilterChip( "AgeInDays", FilterOperator.GreaterThanOrEqualTo, 10 );
	/// </summary>
	public class FilterChip {
		/// <summary>
		/// The field to filter against
		/// </summary>
		public string Field { get; }

		/// <summary>
		/// The operator to perform
		/// </summary>
		public FilterOperator? Operator { get; }

		/// <summary>
		/// The values to filter by
		/// </summary>
		public object[] Values { get; }

		/// <summary>
		/// Model class that defines a filter for a given query
		/// <b>Example:</b><br />
		/// // Find all records where <b>AgeInDays</b> is greater than or equal to 10<br />
		/// var filter = new FilterChip( "AgeInDays", FilterOperator.GreaterThanOrEqualTo, 10 );
		/// </summary>
		/// <param name="field">The field to filter against</param>
		/// <param name="filterOperator">The ope</param>
		/// <param name="values">params style array of values to filter against</param>
		public FilterChip( string field, FilterOperator? filterOperator, params object[] values ) {
			Field = field;
			Operator = filterOperator;
			Values = values;
		}
	}
}
