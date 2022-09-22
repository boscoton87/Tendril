# Tendril.EFCore

## About
Tendril is a Normalized Data Access Layer (DAL) Package. ([See Tendril package](https://www.nuget.org/packages/Tendril))

This package provides extension methods for registering EF Core DbContexts and DbSets with Tendril.

## Example
The following example shows registration of an EF Core DbContext and DbSet with the DataManager class

```C#
public class Student {
	public int Id { get; set; }
	public string Name { get; set; }
}

public class StudentDbContext : DbContext {
	public DbSet<Student> Students { get; set; }
}

var findByFilterService = new LinqFindByFilterService<Student>()
	.WithFilterType( s => s.Id, FilterOperator.EqualTo, v => s => s.Id == v.First() )
	.WithFilterType( s => s.Name, FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.Single() ) )
	.WithFilterType( s => s.Name, FilterOperator.EqualTo, v => s => s.Name == v.Single() )
	.WithFilterType( s => s.Name, FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.Single() ) );

var dataManager = new DataManager()
	.WithDbContext( () => new StudentDbContext( new DbContextOptions() ) )
		.WithDbSet(
			db => db.Students,
			findByFilterService
		);
```