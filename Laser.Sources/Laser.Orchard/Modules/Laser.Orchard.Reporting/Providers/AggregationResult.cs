﻿namespace Laser.Orchard.Reporting.Providers {
    public class AggregationResult
    {
        public string Label { get; set; }
        public double AggregationValue { get; set; }
        public object GroupingField { get; set; }
        public object Other { get; set; }
    }
}