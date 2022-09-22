# Tendril.InMemory

## About
Tendril is a Normalized Data Access Layer (DAL) Package. ([See Tendril package](https://www.nuget.org/packages/Tendril))

This package provides extension methods for registering an in-memory data collection with Tendril.

## Example
The following example shows registration of an in-memory data collection with the DataManager class

```C#
public class Student {
	public int Id { get; set; }
	public string Name { get; set; }
}

var findByFilterService = new LinqFindByFilterService<Student>()
	.WithFilterType( s => s.Id, FilterOperator.EqualTo, v => s => s.Id == v.First() )
	.WithFilterType( s => s.Name, FilterOperator.StartsWith, v => s => s.Name.StartsWith( v.Single() ) )
	.WithFilterType( s => s.Name, FilterOperator.EqualTo, v => s => s.Name == v.Single() )
	.WithFilterType( s => s.Name, FilterOperator.EndsWith, v => s => s.Name.EndsWith( v.Single() ) );

var dataManager = new DataManager()
	.WithInMemoryCache()
		.WithCacheSet(
			new KeyGeneratorSequential<Student, int>(
				( k, _ ) => k + 1,
				s => s.Id,
				( k, s ) => s.Id = k,
			),
			findByFilterService
		);
```