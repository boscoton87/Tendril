# Tendril

## About
Tendril is a Normalized Data Access Layer (DAL) Package.
This package provides a common CRUD interface for underlying collections of data.
In addition to this functionality, Tendril also provides a common query language for decoupling consumption of data from the underlying persistence layer.
This package is intended to be used in conjunction with the corresponding database type packages. These child packages provide functionalities for registering your datasources with Tendril.
Some of these database specific packages include: Tendril.EFCore, Tendril.InMemory.

## Examples
For all examples below, we will be using the following entity definition and dbcontext, note this example is also dependent on Tendril.EFCore for bootstrapping the collection to the DataManager.

```
public class Student {
	public int Id { get; set; }
	public string Name { get; set; }
}

public class StudentDbContext : DbContext {
	public DbSet<Student> Students { get; set; }
}

var findByFilterService = new LinqFindByFilterService<Student>()
	.WithFilterDefinition( "Id", FilterOperator.EqualTo, v => s => s.Id == ( ( int ) v.First() ) )
	.WithFilterDefinition( "Name", FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.Single() as string ) )
	.WithFilterDefinition( "Name", FilterOperator.EqualTo, v => s => s.Name == ( v.Single() as string ) )
	.WithFilterDefinition( "Name", FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.Single() as string ) );

var filterValidator = new FilterChipValidatorService()
	.HasFilterType<int>( "Id", false, 1, 1, FilterOperator.EqualTo )
	.HasFilterType<string>(
		"Name", false, 1, 1,
		FilterOperator.StartsWith, FilterOperator.EqualTo, FilterOperator.EndsWith
	);

var dataManager = new DataManager()
	.WithDbContext( () => new StudentDbContext( new DbContextOptions() ) )
		.WithDbSet(
			db => db.Students,
			filterValidator.ValidateFilters,
			async ( dbSet, filter, page, pageSize ) =>
				await findByFilterService.FindByFilter( dbSet, filter, page, pageSize ).ToListAsync()
		);
```
### Create a new student with the name "John Doe"
`var student = await dataManager.Create( new Student { Name = "John Doe" } );`

### Create multiple students in a batch
```
var students = new List<Student> {
	new Student { Name = "John Doe" },
	new Student { Name = "Jane Doe" }
};
await dataManager.CreateRange( students, 2 );
```

### Insert a student and then change their name
```
var student = await dataManager.Create( new Student { Name = "John Doe" } );
student.Name = "John Lee Doe";
await dataManager.Update( student );
```
### Delete a student from the collection
```
var student = await dataManager.Create( new Student { Name = "John Doe" } );
await dataManager.Delete( student );
```

### Find a student who either has an ID equal to 5, or a name that starts with John and ends with Doe
```
var filters = new OrFilterChip(
	new FilterChip( "Id", FilterOperator.EqualTo, 5 ),
	new AndFilterChip(
		new FilterChip( "Name", FilterOperator.StartsWith, "John" ),
		new FilterChip( "Name", FilterOperator.EndsWith, "Doe" )
	)
);
var students = await DataManager.FindByFilter<Student>( filters );
```