using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Tendril.Enums;
using Tendril.Exceptions;
using Tendril.Models;

namespace Tendril.Services {
	public class LinqFindByFilterService<TModel> where TModel : class {
		public delegate Expression<Func<TModel, bool>> FilterByValues( object[] filterValues );

		private readonly Dictionary<(string field, FilterOperator filterOperator), FilterByValues> _fieldToExpressionBuilder = new();

		public LinqFindByFilterService<TModel> WithFilterDefinition( string field, FilterOperator filterOperator, FilterByValues filterByValues ) {
			_fieldToExpressionBuilder.Add( (field, filterOperator), filterByValues );
			return this;
		}

		public IQueryable<TModel> FindByFilter( IQueryable<TModel> dbSet, FilterChip filter, int? page, int? pageSize ) {
			var query = dbSet.AsQueryable();
			if ( filter is not null ) {
				var predicate = BuildPredicate( filter );
				query = query.Where( predicate );
			}
			if ( page.HasValue && pageSize.HasValue ) {
				query = query.Skip( page.Value * pageSize.Value ).Take( pageSize.Value );
			}
			return query;
		}

		private Expression<Func<TModel, bool>> BuildPredicate( FilterChip filter ) {
			if ( filter is AndFilterChip ) {
				return AndAll( filter.Values.Select( v => BuildPredicate( v as FilterChip ) ).ToArray() );
			} else if ( filter is OrFilterChip ) {
				return OrAny( filter.Values.Select( v => BuildPredicate( v as FilterChip ) ).ToArray() );
			} else if ( filter.Operator.HasValue && _fieldToExpressionBuilder.TryGetValue( (filter.Field, filter.Operator.Value), out FilterByValues filterHandler ) )
				return filterHandler( filter.Values );
			else
				throw new UnsupportedFilterException( $"No {filter.Field} filter definition found for {( filter.Operator.HasValue ? filter.Operator : "null" )} operator" );
		}

		private Expression<Func<TModel, bool>> AndAll( params Expression<Func<TModel, bool>>[] expressions ) {
			if ( expressions == null ) {
				throw new ArgumentNullException( nameof( expressions ) );
			}
			if ( !expressions.Any() ) {
				return t => true;
			}
			var firstPass = true;
			Expression<Func<TModel, bool>> output = null;
			foreach ( var expr in expressions ) {
				if ( firstPass ) {
					output = expr;
					firstPass = false;
				} else {
					var invokedExpr = Expression.Invoke( expr, output.Parameters.Cast<Expression>() );
					output = Expression.Lambda<Func<TModel, bool>>( Expression.AndAlso( output.Body, invokedExpr ), output.Parameters );
				}
			}
			return output;
		}

		private Expression<Func<TModel, bool>> OrAny( params Expression<Func<TModel, bool>>[] expressions ) {
			if ( expressions == null ) {
				throw new ArgumentNullException( nameof( expressions ) );
			}
			if ( !expressions.Any() ) {
				return t => true;
			}
			var firstPass = true;
			Expression<Func<TModel, bool>> output = null;
			foreach ( var expr in expressions ) {
				if ( firstPass ) {
					output = expr;
					firstPass = false;
				} else {
					var invokedExpr = Expression.Invoke( expr, output.Parameters.Cast<Expression>() );
					output = Expression.Lambda<Func<TModel, bool>>( Expression.OrElse( output.Body, invokedExpr ), output.Parameters );
				}
			}
			return output;
		}
	}
}
