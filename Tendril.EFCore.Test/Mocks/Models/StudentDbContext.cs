using Microsoft.EntityFrameworkCore;

namespace Tendril.EFCore.Test.Mocks.Models {
	internal class StudentDbContext : DbContext {
		public StudentDbContext( string connectionString ) : base( new DbContextOptionsBuilder<StudentDbContext>().UseSqlite( connectionString ).Options ) { }

		public DbSet<Student> Students => Set<Student>();
	}
}
