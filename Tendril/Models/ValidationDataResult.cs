namespace Tendril.Models {
	public class ValidationDataResult<TData> : ValidationResult {
		public TData Data { get; init; }
	}
}
