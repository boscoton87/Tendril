using System;

namespace Tendril.InMemory.Services.Interfaces {
	/// <summary>
	/// Interface for service class that generates new keys for storage
	/// </summary>
	/// <typeparam name="TKey">The type of key</typeparam>
	public interface IKeyGenerator<TModel, TKey> where TModel : class where TKey : IComparable {
		public TKey GetNextKey( TModel model );
	}
}
