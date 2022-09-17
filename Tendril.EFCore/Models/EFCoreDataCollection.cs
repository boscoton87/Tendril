using Microsoft.EntityFrameworkCore;
using Tendril.Models;
using Tendril.Services;
using Tendril.Services.Interfaces;

namespace Tendril.EFCore.Models {
	/// <summary>
	/// IDataCollection implementation for Entity Framework Core datasource
	/// </summary>
	/// <typeparam name="TDataSource">The type of DbContext</typeparam>
	/// <typeparam name="TEntity">The type of entity</typeparam>
	internal class EFCoreDataCollection<TDataSource, TEntity> : IDataCollection<TEntity>
		where TDataSource : DbContext
		where TEntity : class {

		private readonly FilterChipValidatorService<TEntity> _filterChipValidator;
		private readonly LinqFindByFilterService<TEntity> _findByFilterService;
		private readonly DataSourceContext<TDataSource> _dataSource;
		private readonly Func<TDataSource, DbSet<TEntity>> _getCollection;

		public EFCoreDataCollection(
			FilterChipValidatorService<TEntity> filterChipValidator,
			LinqFindByFilterService<TEntity> findByFilterService,
			DataSourceContext<TDataSource> dataSource,
			Func<TDataSource, DbSet<TEntity>> getCollection
		) {
			_filterChipValidator = filterChipValidator;
			_findByFilterService = findByFilterService;
			_dataSource = dataSource;
			_getCollection = getCollection;
		}

		public async Task<TEntity> Add( TEntity entity ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollection( dbContext );
			var result = dbSet.Add( entity );
			await dbContext.SaveChangesAsync();
			return result.Entity;
		}

		public async Task AddRange( IEnumerable<TEntity> entities ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollection( dbContext );
			dbSet.AddRange( entities );
			await dbContext.SaveChangesAsync();
		}

		public async Task Delete( TEntity entity ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollection( dbContext );
			dbSet.Remove( entity );
			await dbContext.SaveChangesAsync();
		}

		public async Task<IEnumerable<TEntity>> ExecuteRawQuery( string query, object[] parameters ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollection( dbContext );
			return await dbSet.FromSqlRaw( query, parameters ).ToListAsync();
		}

		public async Task<IEnumerable<TEntity>> FindByFilter( FilterChip filter, int? page = null, int? pageSize = null ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollection( dbContext );
			return await _findByFilterService.FindByFilter( dbSet, filter, page, pageSize ).ToListAsync();
		}

		public async Task<TEntity> Update( TEntity entity ) {
			using var dbContext = _dataSource.GetDataSource();
			var dbSet = _getCollection( dbContext );
			var result = dbSet.Update( entity );
			await dbContext.SaveChangesAsync();
			return result.Entity;
		}

		public ValidationResult ValidateFilters( FilterChip filter ) {
			return _filterChipValidator.ValidateFilters( filter );
		}
	}
}
