using Tendril.InMemory.Delegates;
using Tendril.InMemory.Services.Interfaces;

namespace Tendril.InMemory.Services {
	/// <summary>
	/// Service class that generates sequential keys
	/// </summary>
	/// <typeparam name="TKey">The type of key</typeparam>
	/// /// <typeparam name="TModel">The type of Model</typeparam>
	public class KeyGeneratorSequential<TModel, TKey> : IKeyGenerator<TModel, TKey>
		where TModel : class
		where TKey : IComparable {
		private TKey? _lastKey;

		private readonly GetNextKey<TModel, TKey> _getNextKey;
		private readonly GetKeyFromModel<TModel, TKey> _getKeyFromModel;
		private readonly SetKeyOnModel<TModel, TKey> _setKeyOnModel;

		/// <summary>
		/// Service class that generates sequential keys
		/// </summary>
		/// <param name="getNextKey">Function that given the last key generated, generates a new key</param>
		/// <param name="getKeyFromModel">Function that gets the key from a model</param>
		/// <param name="setKeyOnModel">Function that sets a key on a model</param>
		public KeyGeneratorSequential(
			GetNextKey<TModel, TKey> getNextKey,
			GetKeyFromModel<TModel, TKey> getKeyFromModel,
			SetKeyOnModel<TModel, TKey> setKeyOnModel
		) {
			_lastKey = default;
			_getNextKey = getNextKey;
			_getKeyFromModel = getKeyFromModel;
			_setKeyOnModel = setKeyOnModel;
		}

		/// <summary>
		/// Generates a new key for storage
		/// </summary>
		/// <returns>Key used for storage</returns>
		public TKey GetNextKey( TModel model ) {
			_lastKey = _getNextKey( _lastKey, model );
			return _lastKey;
		}

		/// <summary>
		/// Get the key from the model.
		/// </summary>
		/// <param name="model">The model to pull the key from</param>
		/// <returns>The fetched key</returns>
		public TKey GetKeyFromModel( TModel model ) {
			return _getKeyFromModel( model );
		}

		/// <summary>
		/// Set the key on the model.
		/// </summary>
		/// <param name="key">The key to set</param>
		/// <param name="model">The model to apply the key to</param>
		public void SetKeyOnModel( TKey key, TModel model ) {
			_setKeyOnModel( key, model );
		}
	}
}
