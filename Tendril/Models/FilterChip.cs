using Tendril.Enums;

namespace Tendril.Models {
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
