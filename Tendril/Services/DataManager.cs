using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tendril.Exceptions;
using Tendril.Models;
using Tendril.Models.Interfaces;
using Tendril.Services.Interfaces;

namespace Tendril.Services {
	public class DataManager : IDataManager {
		internal Dictionary<Type, object> TDataSourceToDataSourceContext { get; }
		internal Dictionary<Type, IDataCollection> TModelToCollectionContext { get; }

		public DataManager() {
			TDataSourceToDataSourceContext = new Dictionary<Type, object>();
			TModelToCollectionContext = new Dictionary<Type, IDataCollection>();
		}

		public async Task<TModel> Create<TModel>( TModel source ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			using var dataContext = collectionContext.GetDataContext();
			var entity = collectionContext.Add( dataContext, source );
			await collectionContext.SaveChangesAsync( dataContext );
			return entity;
		}

		public async Task CreateRange<TModel>( IEnumerable<TModel> source, int batchSize ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			using var dataContext = collectionContext.GetDataContext();
			foreach ( var batch in source.Chunk( batchSize ) ) {
				collectionContext.AddRange( dataContext, batch );
				await collectionContext.SaveChangesAsync( dataContext );
			}
		}

		public async Task<TModel> Update<TModel>( TModel source ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			using var dataContext = collectionContext.GetDataContext();
			var entity = collectionContext.Update( dataContext, source );
			await collectionContext.SaveChangesAsync( dataContext );
			return entity;
		}

		public async Task Delete<TModel>( TModel source ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			using var dataContext = collectionContext.GetDataContext();
			collectionContext.Delete( dataContext, source );
			await collectionContext.SaveChangesAsync( dataContext );
		}

		public async Task<IEnumerable<TModel>> FindByFilter<TModel>(
			FilterChip filter = null,
			int? page = null,
			int? pageSize = null
		) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			using var dataContext = collectionContext.GetDataContext();
			var result = await collectionContext.FindByFilter<TModel>( dataContext, filter, page, pageSize );
			if ( !result.IsSuccess ) {
				throw new UnsupportedFilterException( result.Message );
			}
			return result.Data;
		}

		public async Task<IEnumerable<TModel>> ExecuteRawQuery<TModel>( string query, params object[] parameters ) where TModel : class {
			var collectionContext = GetCollectionContext<TModel>();
			using var dataContext = collectionContext.GetDataContext();
			return await collectionContext.ExecuteRawQuery<TModel>( dataContext, query, parameters );
		}

		private IDataCollection GetCollectionContext<TModel>() where TModel : class {
			var modelType = typeof( TModel );
			if ( !TModelToCollectionContext.ContainsKey( modelType ) ) {
				throw new TypeLoadException( $"Model type: {modelType} not registered." );
			}
			return TModelToCollectionContext[ modelType ];
		}
	}
}
