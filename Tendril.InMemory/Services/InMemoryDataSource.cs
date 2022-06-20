using System;
using System.Collections.Generic;

namespace Tendril.InMemory.Services {
	/// <summary>
	/// Service class for in-memory datasource
	/// </summary>
	public class InMemoryDataSource : IDisposable {
		public Dictionary<Type, Dictionary<IComparable, object>> Cache { get; } = new();

		public void Dispose() { }
	}
}
