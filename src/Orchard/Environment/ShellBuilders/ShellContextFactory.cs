using System.Linq;
using Autofac;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Logging;

using System.Diagnostics;

namespace Orchard.Environment.ShellBuilders {
    /// <summary>
    /// High-level coordinator that exercises other component capabilities to
    /// build all of the artifacts for a running shell given a tenant settings.
    /// </summary>
    public interface IShellContextFactory {
        /// <summary>
        /// Builds a shell context given a specific tenant settings structure
        /// </summary>
        ShellContext CreateShellContext(ShellSettings settings);

        /// <summary>
        /// Builds a shell context for an uninitialized Orchard instance. Needed
        /// to display setup user interface.
        /// </summary>
        ShellContext CreateSetupContext(ShellSettings settings);

        /// <summary>
        /// Builds a shell context given a specific description of features and parameters.
        /// Shell's actual current descriptor has no effect. Does not use or update descriptor cache.
        /// </summary>
        ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor);

    }

    public class ShellContextFactory : IShellContextFactory {
        private readonly IShellDescriptorCache _shellDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContextFactory(
            IShellDescriptorCache shellDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _shellDescriptorCache = shellDescriptorCache;
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ShellContext CreateShellContext(ShellSettings settings) {

            var swDescCacheFetch = new Stopwatch();
            var swBPCompose = new Stopwatch();
            var swScopeCreate = new Stopwatch();
            var swDescriptorCreate = new Stopwatch();
            var swDescriptorCreate2 = new Stopwatch();
            var swNewerDescriptor = new Stopwatch();

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);
            swDescCacheFetch.Start();
            var knownDescriptor = _shellDescriptorCache.Fetch(settings.Name);
            swDescCacheFetch.Stop();
            if (knownDescriptor == null) {
                Logger.Information("No descriptor cached. Starting with minimum components.");
                knownDescriptor = MinimumShellDescriptor();
            }
            swBPCompose.Start();
            var blueprint = _compositionStrategy.Compose(settings, knownDescriptor);
            swBPCompose.Stop();
            swScopeCreate.Start();
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
            swScopeCreate.Stop();

            swDescriptorCreate.Start();
            ShellDescriptor currentDescriptor;
            using (var standaloneEnvironment = shellScope.CreateWorkContextScope()) {
                var shellDescriptorManager = standaloneEnvironment.Resolve<IShellDescriptorManager>();
                swDescriptorCreate2.Start();
                currentDescriptor = shellDescriptorManager.GetShellDescriptor();
                swDescriptorCreate2.Stop();
            }
            swDescriptorCreate.Stop();

            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber) {
                swNewerDescriptor.Start();
                Logger.Information("Newer descriptor obtained. Rebuilding shell container.");

                _shellDescriptorCache.Store(settings.Name, currentDescriptor);
                blueprint = _compositionStrategy.Compose(settings, currentDescriptor);
                shellScope.Dispose();
                shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
                swNewerDescriptor.Stop();
            }

            Logger.Error($"CREATESHELLCONTEXT {settings.Name} _shellDescriptorCache.Fetch: {swDescCacheFetch.Elapsed.TotalMilliseconds}");
            Logger.Error($"CREATESHELLCONTEXT {settings.Name} _compositionStrategy.Compose: {swBPCompose.Elapsed.TotalMilliseconds}");
            Logger.Error($"CREATESHELLCONTEXT {settings.Name} _shellContainerFactory.CreateContainer: {swScopeCreate.Elapsed.TotalMilliseconds}");
            Logger.Error($"CREATESHELLCONTEXT {settings.Name} Create descriptor: {swDescriptorCreate.Elapsed.TotalMilliseconds}");
            Logger.Error($"CREATESHELLCONTEXT {settings.Name} shellDescriptorManager.GetShellDescriptor(): {swDescriptorCreate2.Elapsed.TotalMilliseconds}");
            Logger.Error($"CREATESHELLCONTEXT {settings.Name} Newer descriptor: {swNewerDescriptor.Elapsed.TotalMilliseconds}");

            return new ShellContext {
                Settings = settings,
                Descriptor = currentDescriptor,
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }

        private static ShellDescriptor MinimumShellDescriptor() {
            return new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature {Name = "Orchard.Framework"},
                    new ShellFeature {Name = "Settings"},
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }

        public ShellContext CreateSetupContext(ShellSettings settings) {
            Logger.Debug("No shell settings available. Creating shell context for setup");

            var descriptor = new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Setup" },
                    new ShellFeature { Name = "Shapes" },
                    new ShellFeature { Name = "Orchard.Resources" }
                },
            };

            var blueprint = _compositionStrategy.Compose(settings, descriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext {
                Settings = settings,
                Descriptor = descriptor,
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }

        public ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor) {
            Logger.Debug("Creating described context for tenant {0}", settings.Name);

            var blueprint = _compositionStrategy.Compose(settings, shellDescriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext
            {
                Settings = settings,
                Descriptor = shellDescriptor,
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }
    }
}