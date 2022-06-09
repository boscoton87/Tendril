namespace Tendril.Models {
	public class AndFilterChip : FilterChip {
		public AndFilterChip( params FilterChip[] filterChips ) : base( string.Empty, null, filterChips ) { }
	}
}
