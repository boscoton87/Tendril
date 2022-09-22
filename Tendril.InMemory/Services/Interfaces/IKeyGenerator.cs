namespace Tendril.InMemory.Services.Interfaces {
	/// <summary>
	/// Interface for service class that generates new keys for storage
	/// </summary>
	/// <typeparam name="TKey">The type of key</typeparam>
	/// <typeparam name="TModel">The type of model</typeparam>
	public interface IKeyGenerator<TModel, TKey> where TModel : class where TKey : IComparable {
		/// <summary>
		/// Generate the next key to be used
		/// </summary>
		/// <param name="model">The model the key will be applied to. <br />
		/// This is available in the event that info from the model is required to generate the key</param>
		/// <returns></returns>
		public TKey GetNextKey( TModel model );

		/// <summary>
		/// Get the key from the model.
		/// </summary>
		/// <param name="model">The model to pull the key from</param>
		/// <returns>The fetched key</returns>
		public TKey GetKeyFromModel( TModel model );

		/// <summary>
		/// Set the key on the model.
		/// </summary>
		/// <param name="key">The key to set</param>
		/// <param name="model">The model to apply the key to</param>
		public void SetKeyOnModel( TKey key, TModel model );
	}
}
