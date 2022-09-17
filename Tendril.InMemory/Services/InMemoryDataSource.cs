using System;
using System.Collections.Generic;

namespace Tendril.InMemory.Services {
	/// <summary>
	/// Service class for in-memory datasource
	/// </summary>
	public class InMemoryDataSource : IDisposable {
		/// <summary>
		/// The underlying cache for storing the in memory data collections
		/// </summary>
		public Dictionary<Type, Dictionary<IComparable, object>> Cache { get; } = new();

		/// <summary>
		/// Dispose method for implementing the IDisposable interface
		/// </summary>
		public void Dispose() { }
	}
}
