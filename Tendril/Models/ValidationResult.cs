namespace Tendril.Models {
	/// <summary>
	/// Model class containing the results from a validation operation
	/// </summary>
	public class ValidationResult {
		public bool IsSuccess { get; init; } = true;

		public string Message { get; init; } = string.Empty;
	}
}
