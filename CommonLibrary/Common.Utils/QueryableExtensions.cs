using Model.RequestModel.ParametersRequest;
using System.Linq.Expressions;

namespace Common.Utils
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByFields<T>(this IQueryable<T> source, List<SortCriteriaModel> sortCriteria)
        {
            if (sortCriteria == null || !sortCriteria.Any())
            {
                return source;
            }

            IOrderedQueryable<T>? orderedQuery = null;

            for (int i = 0; i < sortCriteria.Count; i++)
            {
                var criteria = sortCriteria[i];

                if (string.IsNullOrWhiteSpace(criteria.Field))
                {
                    continue;
                }

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.PropertyOrField(parameter, criteria.Field);
                var lambda = Expression.Lambda(property, parameter);

                string methodName;
                if (i == 0)
                {
                    methodName = criteria.IsDescending ? "OrderByDescending" : "OrderBy";
                }
                else
                {
                    methodName = criteria.IsDescending ? "ThenByDescending" : "ThenBy";
                }

                var method = typeof(Queryable).GetMethods().Single(m => m.Name == methodName
                                 && m.IsGenericMethodDefinition
                                 && m.GetGenericArguments().Length == 2
                                 && m.GetParameters().Length == 2);

                var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);

                orderedQuery = (IOrderedQueryable<T>)(genericMethod.Invoke(null, new object[] { i == 0 ? source : orderedQuery, lambda }) ?? source);
            }

            return orderedQuery ?? source;
        }

    }
}
