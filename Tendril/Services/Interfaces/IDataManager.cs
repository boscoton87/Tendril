using System.Collections.Generic;
using System.Threading.Tasks;
using Tendril.Models;

namespace Tendril.Services.Interfaces {
	public interface IDataManager {
		Task<TModel> Create<TModel>( TModel source ) where TModel : class;
		Task CreateRange<TModel>( IEnumerable<TModel> source, int batchSize ) where TModel : class;
		Task<IEnumerable<TModel>> FindByFilter<TModel>(
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TModel : class;
		Task<IEnumerable<TModel>> ExecuteRawQuery<TModel>( string query, params object[] parameters ) where TModel : class;
		Task<TModel> Update<TModel>( TModel source ) where TModel : class;
		Task Delete<TModel>( TModel source ) where TModel : class;
	}
}