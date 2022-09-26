using System.ComponentModel.DataAnnotations;

namespace Tendril.EFCore.Test.Mocks.Models {
	internal class Student {
		[Key]
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public bool IsEnrolled { get; set; }

		public DateTime DateOfBirth { get; set; }
	}
}
