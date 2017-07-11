using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.Providers
{
    public interface IGroupByParameterProvider : IDependency
    {
        void Describe(DescribeGroupByContext context);
    }
}