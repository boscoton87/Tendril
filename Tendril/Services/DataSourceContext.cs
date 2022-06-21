﻿using System;
using System.Threading.Tasks;

namespace Tendril.Services {
	/// <summary>
	/// This class is used by the returned by the DataManager fluent interface for defining the data sources to be bootstrapped
	/// </summary>
	/// <typeparam name="TDataSource">The type of connection used for a given datasource<br />
	/// <b>Example:</b> For EFCore this type would be DbContext</typeparam>
	public class DataSourceContext<TDataSource> where TDataSource : IDisposable {
		/// <summary>
		/// Reference to the DataManager instance
		/// </summary>
		public DataManager DataManager { get; init; }

		/// <summary>
		/// Creates a new instance of the underlying connection
		/// </summary>
		public Func<TDataSource> GetDataSource { get; init; }

		/// <summary>
		/// Apply the pending changes to the underlying datasource
		/// </summary>
		public Func<TDataSource, Task> SaveChangesAsync { get; init; }
	}
}
