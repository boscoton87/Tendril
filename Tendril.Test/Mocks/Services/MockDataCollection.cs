using AutoMapper.Internal;
using NUnit.Framework;
using Tendril.Enums;
using Tendril.Models;
using Tendril.Services;
using Tendril.Services.Interfaces;
using Tendril.Test.Mocks.Models;

namespace Tendril.Test.Mocks.Services {
	internal class MockDataCollection : IDataCollection<Student> {
		private readonly List<(string MethodName, object[] args)> _methodCalls;
		private readonly FilterChipValidatorService<Student> _validator;

		public MockDataCollection() {
			_methodCalls = new();
			_validator = new FilterChipValidatorService<Student>()
				.HasFilterType( s => s.Name, false, 1, 1, FilterOperator.EqualTo );
		}

		public void AssertCallMade( int callIndex, int callCount, string methodName, params object[] args ) {
			if ( callIndex >= _methodCalls.Count ) {
				Assert.Fail();
			}
			if ( _methodCalls.Count != callCount ) {
				Assert.Fail();
			}
			var methodCall = _methodCalls[ callIndex ];
			if ( methodCall.MethodName != methodName ) {
				Assert.Fail();
			}
			if ( methodCall.args.Length != args.Length ) {
				Assert.Fail();
			}
			for ( int index = 0; index < methodCall.args.Length; index++ ) {
				if ( !CompareArgs( methodCall.args[ index ], args[ index ] ) ) {
					Assert.Fail();
				}
			}
		}

		public Task<Student> Add( Student entity ) {
			AddCall( "Add", entity );
			return Task.FromResult( entity );
		}

		public Task AddRange( IEnumerable<Student> entities ) {
			AddCall( "AddRange", entities );
			return Task.CompletedTask;
		}

		public Task Delete( Student entity ) {
			AddCall( "Delete", entity );
			return Task.CompletedTask;
		}

		public Task<IEnumerable<Student>> ExecuteRawQuery( string query, params object[] parameters ) {
			AddCall( "ExecuteRawQuery", query, parameters );
			return Task.FromResult( new List<Student> { }.AsEnumerable() );
		}

		public Task<IEnumerable<Student>> FindByFilter( FilterChip filter, int? page = null, int? pageSize = null ) {
			AddCall( "FindByFilter", filter, page, pageSize );
			return Task.FromResult( new List<Student> { }.AsEnumerable() );
		}

		public Task<long> CountByFilter( FilterChip filter, int? page = null, int? pageSize = null ) {
			AddCall( "CountByFilter", filter, page, pageSize );
			return Task.FromResult( 1L );
		}

		public Task<Student> Update( Student entity ) {
			AddCall( "Update", entity );
			return Task.FromResult( entity );
		}

		public ValidationResult ValidateFilters( FilterChip filter ) {
			AddCall( "ValidateFilters", filter );
			return _validator.ValidateFilters( filter );
		}

		private bool CompareArgs( object argA, object argB ) {
			var typeA = argA.GetType();
			var typeB = argB.GetType();
			if ( argA is IEnumerable<Student> && argB is IEnumerable<Student> ) {
				var listA = ( argA as IEnumerable<Student> ).ToList();
				var listB = ( argB as IEnumerable<Student> ).ToList();
				if ( listA.Count != listB.Count ) {
					return false;
				}
				for ( int index = 0; index < listA.Count; index++ ) {
					if ( !CompareArgs( listA[ index ], listB[ index ] ) ) {
						return false;
					}
				}
				return true;
			}
			if ( typeA != typeB ) {
				return false;
			}
			switch ( typeA ) {
				case Type type when type == typeof( Student ):
					var studentA = argA as Student;
					var studentB = argB as Student;
					if ( studentA.Id != studentB.Id ) {
						return false;
					} else if ( studentA.Name != studentB.Name ) {
						return false;
					} else if ( studentA.IsEnrolled != studentB.IsEnrolled ) {
						return false;
					} else if ( studentA.DateOfBirth != studentB.DateOfBirth ) {
						return false;
					} else {
						return true;
					}
				case Type type when type == typeof( string ):
					return ( string ) argA == ( string ) argB;
				case Type type when type == typeof( int ):
					return ( int ) argA == ( int ) argB;
				case Type type when type.GetTypeInheritance().Any( t => t == typeof( FilterChip ) ):
					return ( FilterChip ) argA == ( FilterChip ) argB;
				case Type type when type == typeof( object[] ):
					var argListA = argA as object[];
					var argListB = argB as object[];
					for ( int index = 0; index < argListA.Length; index++ ) {
						if ( !CompareArgs( argListA[ index ], argListB[ index ] ) ) {
							return false;
						}
					}
					return true;
				default:
					return false;
			}
		}

		private void AddCall( string methodName, params object[] args ) {
			_methodCalls.Add( (methodName, args) );
		}
	}
}
