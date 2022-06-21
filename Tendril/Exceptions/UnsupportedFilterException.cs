using System;

namespace Tendril.Exceptions {
	/// <summary>
	/// Exception that is thrown in cases where an unsupported FilterChip was provided
	/// </summary>
	public class UnsupportedFilterException : Exception {
		/// <summary>
		/// Exception that is thrown in cases where an unsupported FilterChip was provided
		/// </summary>
		/// <param name="message">The message for the exception</param>
		public UnsupportedFilterException( string message ) : base( message ) { }
	}
}
