namespace Tendril.Delegates {
	/// <summary>
	/// Convert from one type to another
	/// </summary>
	/// <typeparam name="TIn">Input type</typeparam>
	/// <typeparam name="TOut">Output type</typeparam>
	/// <param name="source">The value to convert</param>
	/// <returns>The converted value</returns>
	public delegate TOut ConvertTo<TIn, TOut>( TIn source )
		where TIn : class
		where TOut : class;
}
