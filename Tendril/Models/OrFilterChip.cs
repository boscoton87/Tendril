namespace Tendril.Models {
	/// <summary>
	/// Model class that represents a conditional <b>OR</b> operation in a query<br />
	/// <b>Example:</b><br />
	/// <code>
	/// // Find all records where their <b>Name</b> starts with <b>A</b> or <b>B</b><br />
	/// var filter = new OrFilterChip(
	///		new FilterChip( "Name", FilterOperator.StartsWith, "A" ),
	///		new FilterChip( "Name", FilterOperator.StartsWith, "B" )
	/// );
	/// </code>
	/// </summary>
	public class OrFilterChip : FilterChip {
		/// <summary>
		/// Model class that represents a conditional <b>OR</b> operation in a query<br />
		/// <b>Example:</b><br />
		/// <code>
		/// // Find all records where their <b>Name</b> starts with <b>A</b> or <b>B</b><br />
		/// var filter = new OrFilterChip(
		///		new FilterChip( "Name", FilterOperator.StartsWith, "A" ),
		///		new FilterChip( "Name", FilterOperator.StartsWith, "B" )
		/// );
		/// </code>
		/// </summary>
		/// <param name="filterChips">params style array of filters to perform <b>OR</b> operation against</param>
		public OrFilterChip( params FilterChip[] filterChips ) : base( string.Empty, null, filterChips ) { }
	}
}
