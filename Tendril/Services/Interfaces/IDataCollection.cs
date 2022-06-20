using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Models;

namespace Tendril.Services.Interfaces {
	public interface IDataCollection {
		public IDisposable GetDataContext();

		public Task SaveChangesAsync( object dataSource );

		public Task<IEnumerable<TEntity>> ExecuteRawQuery<TEntity>( object dataSource, string query, object[] parameters ) where TEntity : class;

		public TEntity Add<TEntity>( object dataSource, TEntity entity ) where TEntity : class;

		public void AddRange<TEntity>( object dataSource, IEnumerable<TEntity> entities ) where TEntity : class;

		public TEntity Update<TEntity>( object dataSource, TEntity entity ) where TEntity : class;

		public void Delete<TEntity>( object dataSource, TEntity entity ) where TEntity : class;

		public Task<ValidationDataResult<IEnumerable<TEntity>>> FindByFilter<TEntity>(
			object dataSource,
			FilterChip filter,
			int? page = null,
			int? pageSize = null
		) where TEntity : class;
	}
}
