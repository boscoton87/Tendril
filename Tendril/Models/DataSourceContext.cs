using System;
using System.Threading.Tasks;
using Tendril.Services;

namespace Tendril.Models {
	public class DataSourceContext<TDataSource> where TDataSource : IDisposable {
		internal DataManager DataManager { get; init; }

		public Func<TDataSource> GetDataSource { get; init; }

		public Func<TDataSource, Task> SaveChangesAsync { get; init; }
	}
}
