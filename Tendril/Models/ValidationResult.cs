namespace Tendril.Models {
	/// <summary>
	/// Model class containing the results from a validation operation
	/// </summary>
	public class ValidationResult {
		/// <summary>
		/// Indicates whether the operation was successful
		/// </summary>
		public bool IsSuccess { get; init; } = true;

		/// <summary>
		/// If non-successful, this property will contain the associated message
		/// </summary>
		public string Message { get; init; } = string.Empty;
	}
}
