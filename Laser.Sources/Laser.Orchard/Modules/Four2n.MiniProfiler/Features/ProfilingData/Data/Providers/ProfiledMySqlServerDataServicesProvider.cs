// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfiledMySqlServerDataServicesProvider.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the ProfiledMySqlServerDataServicesProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Features.ProfilingData.Data.Providers
{
    using System.Diagnostics;
    using FluentNHibernate.Cfg.Db;
    using global::Orchard.Environment.Extensions;

    [OrchardFeature(FeatureNames.ProfilingData)]
    [OrchardSuppressDependency("Orchard.Data.Providers.MySqlDataServicesProvider")]
    public class ProfiledMySqlServerDataServicesProvider : global::Orchard.Data.Providers.MySqlDataServicesProvider
    {
        public ProfiledMySqlServerDataServicesProvider(string dataFolder, string connectionString)
            : base(dataFolder, connectionString)
        {
        }

        public static new string ProviderName
        {
            get { return global::Orchard.Data.Providers.MySqlDataServicesProvider.ProviderName; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase)
        {
            var persistence = (MySQLConfiguration)base.GetPersistenceConfigurer(createDatabase);
            Debug.WriteLine("[Four2n.MiniProfiler] - ProfiledMySqlDataServicesProvider - GetPersistenceConfigurer ");
            return persistence.Driver(typeof(ProfiledMySqlClientDriver).AssemblyQualifiedName);
        }
    }
}