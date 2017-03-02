// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyRecord.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the DummyRecord type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Models
{
    using Four2n.Orchard.MiniProfiler.Features;

    using global::Orchard.ContentManagement.Records;
    using global::Orchard.Environment.Extensions;

    /// <summary>
    /// Dummy record for including this module in data configuring.
    /// </summary>
    [OrchardFeature(FeatureNames.ProfilingData)]
    public class DummyRecord : ContentPartRecord
    {
    }
}