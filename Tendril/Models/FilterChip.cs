using Tendril.Enums;

namespace Tendril.Models {
	/// <summary>
	/// Abstract model class that defines a filter for a given query
	/// </summary>
	public abstract class FilterChip {
		/// <summary>
		/// The field to filter against
		/// </summary>
		public string Field { get; }

		/// <summary>
		/// The operation to perform
		/// </summary>
		public FilterOperator? Operator { get; }

		/// <summary>
		/// The values to filter by
		/// </summary>
		public object[] Values { get; }

		internal FilterChip( string field, FilterOperator? filterOperator, params object[] values ) {
			Field = field;
			Operator = filterOperator;
			Values = values;
		}
	}
}
