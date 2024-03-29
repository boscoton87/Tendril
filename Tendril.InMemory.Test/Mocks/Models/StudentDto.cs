﻿namespace Tendril.InMemory.Test.Mocks.Models {
	internal class StudentDto {
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public bool IsEnrolled { get; set; }

		public DateTime DateOfBirth { get; set; }
	}
}
