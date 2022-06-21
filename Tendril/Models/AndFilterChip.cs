namespace Tendril.Models {
	/// <summary>
	/// Model class that represents a conditional <b>AND</b> operation in a query<br />
	/// <b>Example:</b><br />
	/// <code>
	/// // Find all records where their <b>Name</b> starts with <b>A</b> and their <b>Occupation</b> is one of the following: <b>Doctor, Lawyer</b><br />
	/// var filter = new AndFilterChip(
	///		new FilterChip( "Name", FilterOperator.StartsWith, "A" ),
	///		new FilterChip( "Occupation", FilterOperator.In, "Doctor", "Lawyer" )
	/// );
	/// </code>
	/// </summary>
	public class AndFilterChip : FilterChip {
		/// <summary>
		/// Model class that represents a conditional <b>AND</b> operation in a query<br />
		/// <b>Example:</b><br />
		/// <code>
		/// // Find all records where their <b>Name</b> starts with <b>A</b> and their <b>Occupation</b> is one of the following: <b>Doctor, Lawyer</b><br />
		/// var filter = new AndFilterChip(
		///		new FilterChip( "Name", FilterOperator.StartsWith, "A" ),
		///		new FilterChip( "Occupation", FilterOperator.In, "Doctor", "Lawyer" )
		/// );
		/// </code>
		/// </summary>
		/// <param name="filterChips">params style array of filters to perform <b>AND</b> operation against</param>
		public AndFilterChip( params FilterChip[] filterChips ) : base( string.Empty, null, filterChips ) { }
	}
}
