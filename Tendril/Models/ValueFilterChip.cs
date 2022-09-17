using System;
using System.Linq;
using System.Linq.Expressions;
using Tendril.Enums;
using Tendril.Extensions;

namespace Tendril.Models {
	/// <summary>
	/// Model class that defines a filter for a given query
	/// <b>Example:</b><br />
	/// // Find all records where <b>AgeInDays</b> is greater than or equal to 10<br />
	/// <code>
	/// var filter = new ValueFilterChip&lt;SomeModel, int&gt;( m =&gt; m.AgeInDays, FilterOperator.GreaterThanOrEqualTo, 10 );
	/// </code>
	/// </summary>
	public class ValueFilterChip<TModel, TValue> : FilterChip where TModel : class {
		/// <summary>
		/// Model class that defines a filter for a given query
		/// <b>Example:</b><br />
		/// // Find all records where <b>AgeInDays</b> is greater than or equal to 10<br />
		/// <code>
		/// var filter = new ValueFilterChip&lt;SomeModel, int&gt;( m =&gt; m.AgeInDays, FilterOperator.GreaterThanOrEqualTo, 10 );
		/// </code>
		/// </summary>
		/// <param name="getProperty">Expression to get the targeted property from the given model type</param>
		/// <param name="filterOperator">The operation to perform</param>
		/// <param name="values">params style array of values to filter against</param>
		public ValueFilterChip( Expression<Func<TModel, TValue>> getProperty, FilterOperator filterOperator, params TValue[] values )
			: base( getProperty.GetPropertyName(), filterOperator, values?.Select( v => ( object ) v ).ToArray() ) { }

		/// <summary>
		/// Model class that defines a filter for a given query
		/// <b>Example:</b><br />
		/// // Find all records where <b>AgeInDays</b> is greater than or equal to 10<br />
		/// var filter = new FilterChip( "AgeInDays", FilterOperator.GreaterThanOrEqualTo, 10 );
		/// </summary>
		/// <param name="field">The field to filter against</param>
		/// <param name="filterOperator">The operation to perform</param>
		/// <param name="values">params style array of values to filter against</param>
		public ValueFilterChip( string field, FilterOperator filterOperator, params TValue[] values )
			: base( field, filterOperator, values?.Select( v => ( object ) v ).ToArray() ) { }
	}
}
