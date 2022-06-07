﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tendril.Test.Mocks.Models {
	internal class Student {
		public int Id { get; set; }

		public string Name { get; set; }

		public bool IsEnrolled { get; set; }

		public DateTime DateOfBirth { get; set; }
	}
}
