﻿namespace Tendril.Models {
	/// <summary>
	/// Model class containing the results from a validation operation
	/// </summary>
	/// <typeparam name="TData">The type of data in the Data property</typeparam>
	public class ValidationDataResult<TData> : ValidationResult {
		/// <summary>
		/// The data for the validation result
		/// </summary>
		public TData Data { get; init; }
	}
}
