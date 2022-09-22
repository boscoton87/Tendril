namespace Tendril.InMemory.Delegates {
	/// <summary>
	/// Delegate defining how to get the next generated key.
	/// </summary>
	/// <param name="previousKey">The previous key that was generated</param>
	/// <param name="model">The model this key will be applied to. 
	/// The model is provided in case values from it are required for calculating the key</param>
	/// <returns>The selected DbSet</returns>
	public delegate TKey GetNextKey<TModel, TKey>( TKey? previousKey, TModel model )
		where TModel : class
		where TKey : IComparable;

	/// <summary>
	/// Get the key from the model.
	/// </summary>
	/// <param name="model">The model to pull the key from</param>
	/// <returns>The fetched key</returns>
	public delegate TKey GetKeyFromModel<TModel, TKey>( TModel model )
		where TModel : class
		where TKey : IComparable;

	/// <summary>
	/// Set the key on the model.
	/// </summary>
	/// <param name="key">The key to set</param>
	/// <param name="model">The model to apply the key to</param>
	public delegate void SetKeyOnModel<TModel, TKey>( TKey key, TModel model )
		where TModel : class
		where TKey : IComparable;
}
