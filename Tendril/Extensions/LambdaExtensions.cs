using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Tendril.Extensions {
	/// <summary>
	/// Extension methods for getting a property name from a lambda expression
	/// </summary>
	internal static class LambdaExtensions {
		public static PropertyInfo GetPropertyInfo<TType, TReturn>(
			this Expression<Func<TType, TReturn>> property
		) {
			LambdaExpression lambda = property;
			var memberExpression = lambda.Body is UnaryExpression expression
				? ( MemberExpression ) expression.Operand
				: ( MemberExpression ) lambda.Body;

			return ( PropertyInfo ) memberExpression.Member;
		}

		public static string GetPropertyName<TType, TReturn>(
			this Expression<Func<TType, TReturn>> property
		) {
			return property.GetPropertyInfo().Name;
		}
	}
}
