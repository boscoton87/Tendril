using System;
using System.Threading.Tasks;

namespace Tendril.Services {
	public class DataSourceContext<TDataSource> where TDataSource : IDisposable {
		internal DataManager DataManager { get; init; }

		public Func<TDataSource> GetDataSource { get; init; }

		public Func<TDataSource, Task> SaveChangesAsync { get; init; }
	}
}
