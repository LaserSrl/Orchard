using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Projections {
    public static class DefaultHqlExpressionFactoryExtensions {
        public static void InSubquery(this IHqlExpressionFactory hqlExpressionFactory, string propertyName, string subquery) {
            var aux = (hqlExpressionFactory as DefaultHqlExpressionFactory);
            var crit = InSubquery(propertyName, subquery);
            var property = typeof(DefaultHqlExpressionFactory).GetProperty("Criterion");
            property.SetValue(aux, crit);
        }
        private static IHqlCriterion InSubquery(string propertyName, string subquery) {
            if (string.IsNullOrWhiteSpace(subquery)) {
                throw new ArgumentException("Subquery can't be empty", "subquery");
            }
            return new BinaryExpression("in", propertyName, "(" + subquery + ")");
        }
    }
}