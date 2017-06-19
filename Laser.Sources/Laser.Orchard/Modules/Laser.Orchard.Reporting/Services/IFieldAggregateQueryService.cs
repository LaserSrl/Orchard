using Orchard.ContentManagement;
using Laser.Orchard.Reporting.Models;
using Laser.Orchard.Reporting.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Laser.Orchard.Reporting.Services
{
    public interface IFieldAggregateQueryService : IDependency
    {
        IEnumerable<AggregationResult> RunNumericAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName, int interval);
        IEnumerable<AggregationResult> RunEnumerationAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName);
        IEnumerable<AggregationResult> RunBooleanAggregation(IHqlQuery query, AggregateMethods aggregateMethod, string fieldName, string partName);
    }
}
