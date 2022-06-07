using System;

namespace Tendril.Exceptions {
	public class UnsupportedFilterException : Exception {
		public UnsupportedFilterException( string message ) : base( message ) { }
	}
}
