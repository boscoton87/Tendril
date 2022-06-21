namespace Tendril.Enums {
	/// <summary>
	/// Enum that states which type of filter operation to perform
	/// </summary>
	public enum FilterOperator {

		/// <summary>
		/// Less Than Operator<br />
		/// <b>Example:</b><br />
		/// A &lt; 5
		/// </summary>
		LessThan,

		/// <summary>
		/// Less Than or Equal To Operator<br />
		/// <b>Example:</b><br />
		/// A &lt;= 5
		/// </summary>
		LessThanOrEqualTo,

		/// <summary>
		/// Equal To Operator<br />
		/// <b>Example:</b><br />
		/// A == 5
		/// </summary>
		EqualTo,

		/// <summary>
		/// Not Equal To Operator<br />
		/// <b>Example:</b><br />
		/// A != 5
		/// </summary>
		NotEqualTo,

		/// <summary>
		/// Greater Than or Equal To Operator<br />
		/// <b>Example:</b><br />
		/// A &gt;= 5
		/// </summary>
		GreaterThanOrEqualTo,

		/// <summary>
		/// Greater Than Operator<br />
		/// <b>Example:</b><br />
		/// A &gt; 5
		/// </summary>
		GreaterThan,

		/// <summary>
		/// Starts With Operator<br />
		/// <b>Example:</b><br />
		/// A LIKE "foo%"
		/// </summary>
		StartsWith,

		/// <summary>
		/// Not Starts With Operator<br />
		/// <b>Example:</b><br />
		/// A NOT LIKE "foo%"
		/// </summary>
		NotStartsWith,

		/// <summary>
		/// Contains Operator<br />
		/// <b>Example:</b><br />
		/// A LIKE "%foo%"
		/// </summary>
		Contains,

		/// <summary>
		/// Not Contains Operator<br />
		/// <b>Example:</b><br />
		/// A NOT LIKE "%foo%"
		/// </summary>
		NotContains,

		/// <summary>
		/// Ends With Operator<br />
		/// <b>Example:</b><br />
		/// A LIKE "%foo"
		/// </summary>
		EndsWith,

		/// <summary>
		/// Not Ends With Operator<br />
		/// <b>Example:</b><br />
		/// A NOT LIKE "%foo"
		/// </summary>
		NotEndsWith,

		/// <summary>
		/// In Operator<br />
		/// <b>Example:</b><br />
		/// A IN ( 1, 2, 3 )
		/// </summary>
		In,

		/// <summary>
		/// Not In Operator<br />
		/// <b>Example:</b><br />
		/// A NOT IN ( 1, 2, 3 )
		/// </summary>
		NotIn
	}
}
