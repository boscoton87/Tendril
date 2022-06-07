using System;
using System.Collections.Generic;

namespace Tendril.InMemory.Models {
	public class InMemoryDataSource : IDisposable {
		public Dictionary<Type, Dictionary<IComparable, object>> Cache { get; } = new Dictionary<Type, Dictionary<IComparable, object>>();

		public void Dispose() {
			
		}
	}
}
