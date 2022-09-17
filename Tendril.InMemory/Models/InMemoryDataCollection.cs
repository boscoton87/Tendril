using Tendril.InMemory.Services.Interfaces;
using Tendril.Models;
using Tendril.Services;
using Tendril.Services.Interfaces;

namespace Tendril.InMemory.Models {
	/// <summary>
	/// IDataCollection implementation for in-memory datasource
	/// </summary>
	/// <typeparam name="TModel">The type of model</typeparam>
	/// <typeparam name="TKey">The type of key</typeparam>
	internal class InMemoryDataCollection<TModel, TKey> : IDataCollection<TModel>
		where TModel : class
		where TKey : IComparable {

		private readonly FilterChipValidatorService<TModel> _filterChipValidator;
		private readonly LinqFindByFilterService<TModel> _findByFilterService;
		private readonly Dictionary<IComparable, object> _collection;
		private readonly Func<TModel, TKey> _getKey;
		private readonly Action<TKey, TModel> _setKey;
		private readonly IKeyGenerator<TModel, TKey> _keyGenerator;

		public InMemoryDataCollection(
			FilterChipValidatorService<TModel> filterChipValidator,
			LinqFindByFilterService<TModel> findByFilterService,
			Func<TModel, TKey> getKey,
			Action<TKey, TModel> setKey,
			IKeyGenerator<TModel, TKey> keyGenerator,
			Dictionary<IComparable, object> collection
		) {
			_filterChipValidator = filterChipValidator;
			_findByFilterService = findByFilterService;
			_collection = collection;
			_getKey = getKey;
			_setKey = setKey;
			_keyGenerator = keyGenerator;
		}

		public Task<TModel> Add( TModel entity ) {
			_setKey( _keyGenerator.GetNextKey( entity ), entity );
			var key = _getKey( entity );
			_collection.Add( key, entity );
			return Task.FromResult( entity );
		}

		public Task AddRange( IEnumerable<TModel> entities ) {
			foreach ( var entity in entities ) {
				Add( entity );
			}
			return Task.CompletedTask;
		}

		public Task Delete( TModel entity ) {
			var key = _getKey( entity );
			_collection.Remove( key );
			return Task.CompletedTask;
		}

		public Task<IEnumerable<TModel>> ExecuteRawQuery( string query, object[] parameters ) {
			throw new NotSupportedException();
		}

		public Task<IEnumerable<TModel>> FindByFilter( FilterChip filter, int? page = null, int? pageSize = null ) {
			return Task.FromResult(
					_findByFilterService.FindByFilter(
					_collection.Values.Select( v => ( TModel ) v ).AsQueryable(),
					filter,
					page,
					pageSize
				).ToList().AsEnumerable()
			);
		}

		public Task<TModel> Update( TModel entity ) {
			var key = _getKey( entity );
			_ = _collection.Keys.Single( k => k.CompareTo( key ) == 0 );
			_collection[ key ] = entity;
			return Task.FromResult( entity );
		}

		public ValidationResult ValidateFilters( FilterChip filter ) {
			return _filterChipValidator.ValidateFilters( filter );
		}
	}
}
