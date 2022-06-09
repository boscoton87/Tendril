namespace Tendril.Models {
	public class OrFilterChip : FilterChip {
		public OrFilterChip( params FilterChip[] filterChips ) : base( string.Empty, null, filterChips ) { }
	}
}
