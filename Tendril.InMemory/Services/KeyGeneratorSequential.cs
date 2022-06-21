using Tendril.InMemory.Services.Interfaces;

namespace Tendril.InMemory.Services {
	/// <summary>
	/// Service class that generates sequential keys
	/// </summary>
	/// <typeparam name="TKey">The type of key</typeparam>
	/// /// <typeparam name="TModel">The type of Model</typeparam>
	public class KeyGeneratorSequential<TModel, TKey> : IKeyGenerator<TModel, TKey> where TModel : class where TKey : IComparable {
		private TKey? _lastKey;

		private readonly Func<TKey?, TModel, TKey> _getNextKey;

		/// <summary>
		/// Service class that generates sequential keys
		/// </summary>
		/// <param name="getNextKey">Function that given the last key generated, generates a new key</param>
		public KeyGeneratorSequential( Func<TKey?, TModel, TKey> getNextKey ) {
			_lastKey = default;
			_getNextKey = getNextKey;
		}

		/// <summary>
		/// Generates a new key for storage
		/// </summary>
		/// <returns>Key used for storage</returns>
		public TKey GetNextKey( TModel model ) {
			_lastKey = _getNextKey( _lastKey, model );
			return _lastKey;
		}
	}
}
