using System;
using System.Threading.Tasks;

namespace Tendril.Services {
	/// <summary>
	/// This class is used by the returned by the DataManager fluent interface for defining the data sources to be bootstrapped
	/// </summary>
	/// <typeparam name="TDataSource">The type of connection used for a given datasource<br />
	/// <b>Example:</b> For EFCore this type would be DbContext</typeparam>
	public class DataSourceContext<TDataSource> where TDataSource : IDisposable {
		internal DataManager DataManager { get; init; }

		internal Func<TDataSource> GetDataSource { get; init; }

		internal Func<TDataSource, Task> SaveChangesAsync { get; init; }
	}
}
