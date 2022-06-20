using System;

namespace Tendril.Exceptions {
	/// <summary>
	/// Exception that is thrown in cases where an unsupported FilterChip was provided
	/// </summary>
	public class UnsupportedFilterException : Exception {
		public UnsupportedFilterException( string message ) : base( message ) { }
	}
}
