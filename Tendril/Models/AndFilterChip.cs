using Tendril.Enums;

namespace Tendril.Models {
	public class AndFilterChip : FilterChip {
		public AndFilterChip( params FilterChip[] filterChips ) : base( string.Empty, FilterOperator.And, filterChips ) { }
	}
}
