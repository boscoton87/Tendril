using Tendril.InMemory.Services.Interfaces;
using Tendril.Models;
using Tendril.Services;
using Tendril.Services.Interfaces;

namespace Tendril.InMemory.Services {
	/// <summary>
	/// IDataCollection implementation for in-memory datasource
	/// </summary>
	/// <typeparam name="TModel">The type of model</typeparam>
	/// <typeparam name="TKey">The type of key</typeparam>
	internal class InMemoryDataCollection<TModel, TKey> : IDataCollection<TModel>
		where TModel : class
		where TKey : IComparable {

		private readonly LinqFindByFilterService<TModel> _findByFilterService;
		private readonly Dictionary<IComparable, object> _collection;
		private readonly IKeyGenerator<TModel, TKey> _keyGenerator;

		public InMemoryDataCollection(
			LinqFindByFilterService<TModel> findByFilterService,
			IKeyGenerator<TModel, TKey> keyGenerator,
			Dictionary<IComparable, object> collection
		) {
			_findByFilterService = findByFilterService;
			_collection = collection;
			_keyGenerator = keyGenerator;
		}

		public Task<TModel> Add( TModel entity ) {
			_keyGenerator.SetKeyOnModel( _keyGenerator.GetNextKey( entity ), entity );
			var key = _keyGenerator.GetKeyFromModel( entity );
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
			var key = _keyGenerator.GetKeyFromModel( entity );
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
			var key = _keyGenerator.GetKeyFromModel( entity );
			_ = _collection.Keys.Single( k => k.CompareTo( key ) == 0 );
			_collection[ key ] = entity;
			return Task.FromResult( entity );
		}

		public ValidationResult ValidateFilters( FilterChip filter ) {
			return _findByFilterService.ValidateFilters( filter );
		}
	}
}
