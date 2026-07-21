using System.Linq.Expressions;
using System.Reflection;

namespace NashunumaApp.Infrastructure.Common
{
    public static class SearchHelper
    {
        public static Expression<Func<T, bool>> BuildPredicate<T>(string searchTerm)
        {
            searchTerm = searchTerm.Trim().ToLower();

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                Expression? propertyExpression = null;

                if (property.PropertyType == typeof(string))
                {
                    var propertyAccess = Expression.Property(parameter, property);

                    var notNull = Expression.NotEqual(
                        propertyAccess,
                        Expression.Constant(null, typeof(string)));

                    var toLower = Expression.Call(
                        propertyAccess,
                        typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);

                    var contains = Expression.Call(
                        toLower,
                        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                        Expression.Constant(searchTerm));

                    propertyExpression = Expression.AndAlso(notNull, contains);
                }

                if (propertyExpression != null)
                {
                    body = body == null
                        ? propertyExpression
                        : Expression.OrElse(body, propertyExpression);
                }
            }

            body ??= Expression.Constant(true);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}