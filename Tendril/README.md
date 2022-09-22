# Tendril

## About
Tendril is a Normalized Data Access Layer (DAL) Package.
This package provides a common CRUD interface for underlying collections of data.
In addition to this functionality, Tendril also provides a common query language for decoupling consumption of data from the underlying persistence layer.
This package is intended to be used in conjunction with the corresponding database type packages. These child packages provide functionalities for registering your datasources with Tendril.
Some of these datasource specific packages include: [Tendril.EFCore](https://www.nuget.org/packages/Tendril.EFCore), [Tendril.InMemory](https://www.nuget.org/packages/Tendril.InMemory).

## Examples
For all examples below, we will be using the following entity definition and dbcontext, note this example is also dependent on Tendril.EFCore for bootstrapping the collection to the DataManager.

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
### Create a new student with the name "John Doe"
```C#
var student = await dataManager.Create( new Student { Name = "John Doe" } );
```

### Create multiple students in a batch
```C#
var students = new List<Student> {
	new Student { Name = "John Doe" },
	new Student { Name = "Jane Doe" }
};
await dataManager.CreateRange( students );
```

### Insert a student and then change their name
```C#
var student = await dataManager.Create( new Student { Name = "John Doe" } );
student.Name = "John Lee Doe";
await dataManager.Update( student );
```
### Delete a student from the collection
```C#
var student = await dataManager.Create( new Student { Name = "John Doe" } );
await dataManager.Delete( student );
```

### Find a student who either has an ID equal to 5, or a name that starts with John and ends with Doe
```C#
var filters = new OrFilterChip(
	new ValueFilterChip<Student, int>( s => s.Id, FilterOperator.EqualTo, 5 ),
	new AndFilterChip(
		new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.StartsWith, "John" ),
		new ValueFilterChip<Student, string>( s => s.Name, FilterOperator.EndsWith, "Doe" )
	)
);
var students = await DataManager.FindByFilter<Student>( filters );
```