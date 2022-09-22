using Microsoft.EntityFrameworkCore;
using Tendril.EFCore.Delegates;
using Tendril.Models;
using Tendril.Services;
using Tendril.Services.Interfaces;

namespace Tendril.EFCore.Services {
	/// <summary>
	/// IDataCollection implementation for Entity Framework Core datasource
	/// </summary>
	/// <typeparam name="TDataSource">The type of DbContext</typeparam>
	/// <typeparam name="TEntity">The type of entity</typeparam>
	internal class EFCoreDataCollection<TDataSource, TEntity> : IDataCollection<TEntity>
		where TDataSource : DbContext
		where TEntity : class {

		private readonly LinqFindByFilterService<TEntity> _findByFilterService;
		private readonly DataSourceContext<TDataSource> _dataSource;
		private readonly GetDbSet<TDataSource, TEntity> _getCollection;
		private readonly GetCollectionQueryable<TDataSource, TEntity> _getCollectionAsQueryable;
		private readonly AddEntity<TDataSource, TEntity>? _addOverride;
		private readonly AddEntities<TDataSource, TEntity>? _addRangeOverride;
		private readonly DeleteEntity<TDataSource, TEntity>? _deleteOverride;
		private readonly FindByRawQuery<TDataSource, TEntity>? _executeRawQueryOverride;
		private readonly UpdateEntity<TDataSource, TEntity>? _updateOverride;

		public EFCoreDataCollection(
			LinqFindByFilterService<TEntity> findByFilterService,
			DataSourceContext<TDataSource> dataSource,
			GetDbSet<TDataSource, TEntity> getCollection,
			GetCollectionQueryable<TDataSource, TEntity>? getCollectionAsQueryable = null,
			AddEntity<TDataSource, TEntity>? addOverride = null,
			AddEntities<TDataSource, TEntity>? addRangeOverride = null,
			DeleteEntity<TDataSource, TEntity>? deleteOverride = null,
			FindByRawQuery<TDataSource, TEntity>? executeRawQueryOverride = null,
			UpdateEntity<TDataSource, TEntity>? updateOverride = null
		) {
			_findByFilterService = findByFilterService;
			_dataSource = dataSource;
			_getCollection = getCollection;
			_getCollectionAsQueryable = getCollectionAsQueryable ?? ( db => getCollection( db ).AsQueryable() );
			_addOverride = addOverride;
			_addRangeOverride = addRangeOverride;
			_deleteOverride = deleteOverride;
			_executeRawQueryOverride = executeRawQueryOverride;
			_updateOverride = updateOverride;
		}

		public async Task<TEntity> Add( TEntity entity ) {
			using var dbContext = _dataSource.GetDataSource();
			if ( _addOverride is not null ) {
				return await _addOverride( dbContext, entity );
			}
			var dbSet = _getCollection( dbContext );
			var result = dbSet.Add( entity );
			await dbContext.SaveChangesAsync();
			return result.Entity;
		}

		public async Task AddRange( IEnumerable<TEntity> entities ) {
			using var dbContext = _dataSource.GetDataSource();
			if ( _addRangeOverride is not null ) {
				await _addRangeOverride( dbContext, entities );
				return;
			}
			var dbSet = _getCollection( dbContext );
			dbSet.AddRange( entities );
			await dbContext.SaveChangesAsync();
		}

		public async Task Delete( TEntity entity ) {
			using var dbContext = _dataSource.GetDataSource();
			if ( _deleteOverride is not null ) {
				await _deleteOverride( dbContext, entity );
				return;
			}
			var dbSet = _getCollection( dbContext );
			dbSet.Remove( entity );
			await dbContext.SaveChangesAsync();
		}

		public async Task<IEnumerable<TEntity>> ExecuteRawQuery( string query, object[] parameters ) {
			using var dbContext = _dataSource.GetDataSource();
			if ( _executeRawQueryOverride is not null ) {
				return await _executeRawQueryOverride( dbContext, query, parameters );
			}
			var dbSet = _getCollection( dbContext );
			return await dbSet.FromSqlRaw( query, parameters ).ToListAsync();
		}

		public async Task<IEnumerable<TEntity>> FindByFilter( FilterChip filter, int? page = null, int? pageSize = null ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollectionAsQueryable( dbContext );
			return await _findByFilterService.FindByFilter( dbSet, filter, page, pageSize ).ToListAsync();
		}

		public async Task<TEntity> Update( TEntity entity ) {
			using var dbContext = _dataSource.GetDataSource();
			if ( _updateOverride is not null ) {
				return await _updateOverride( dbContext, entity );
			}
			var dbSet = _getCollection( dbContext );
			var result = dbSet.Update( entity );
			await dbContext.SaveChangesAsync();
			return result.Entity;
		}

		public ValidationResult ValidateFilters( FilterChip filter ) {
			return _findByFilterService.ValidateFilters( filter );
		}
	}
}
