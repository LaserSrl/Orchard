using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using log4net.Repository.Hierarchy;
using NHibernate.Util;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Utility;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    // All the event handlers that DefaultOrchardHost implements have to be declared in OrchardStarter.
    public class DefaultOrchardHost : IOrchardHost, IShellSettingsManagerEventHandler, IShellDescriptorManagerEventHandler {
        private readonly IHostLocalRestart _hostLocalRestart;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IProcessingEngine _processingEngine;
        private readonly IExtensionLoaderCoordinator _extensionLoaderCoordinator;
        private readonly IExtensionMonitoringCoordinator _extensionMonitoringCoordinator;
        private readonly ICacheManager _cacheManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly static object _shellContextsWriteLock = new object();
        private readonly NamedReaderWriterLock _shellActivationLock = new NamedReaderWriterLock();

        private readonly ContextState<IList<ShellSettings>> _tenantsToRestart;

        private readonly ConcurrentDictionary<string, ShellContext> _shellContextsDictionary
            = new ConcurrentDictionary<string, ShellContext>(StringComparer.OrdinalIgnoreCase);
        // The locks used for initialization/disposal of shells.
        readonly static ReaderWriterLockSlim _initializationLock =
            new ReaderWriterLockSlim();
        readonly static ReaderWriterLockSlim _shellDisposalLock
            = new ReaderWriterLockSlim();

        public int Retries { get; set; }
        public bool DelayRetries { get; set; }

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IProcessingEngine processingEngine,
            IExtensionLoaderCoordinator extensionLoaderCoordinator,
            IExtensionMonitoringCoordinator extensionMonitoringCoordinator,
            ICacheManager cacheManager,
            IHostLocalRestart hostLocalRestart, 
            IHttpContextAccessor httpContextAccessor) {

            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _processingEngine = processingEngine;
            _extensionLoaderCoordinator = extensionLoaderCoordinator;
            _extensionMonitoringCoordinator = extensionMonitoringCoordinator;
            _cacheManager = cacheManager;
            _hostLocalRestart = hostLocalRestart;
            _httpContextAccessor = httpContextAccessor;

            _tenantsToRestart = new ContextState<IList<ShellSettings>>("DefaultOrchardHost.TenantsToRestart", () => new List<ShellSettings>());

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IList<ShellContext> Current {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        public ShellContext GetShellContext(ShellSettings shellSettings) {
            return BuildCurrent().SingleOrDefault(shellContext => shellContext.Settings.Name.Equals(shellSettings.Name));
        }

        void IOrchardHost.Initialize() {
            var httpContext = _httpContextAccessor.Current();
            Logger.Error($"Initialize() {httpContext?.Request.Path ?? "null context"} started");
            Logger.Information("Initializing");
            BuildCurrent();
            Logger.Information("Initialized");
            Logger.Error($"Initialize() {httpContext?.Request.Path ?? "null context"} done");
        }

        void IOrchardHost.ReloadExtensions() {
            DisposeShellContext();
        }

        void IOrchardHost.BeginRequest() {
            Logger.Debug("BeginRequest");
            BeginRequest();
        }

        void IOrchardHost.EndRequest() {
            Logger.Debug("EndRequest");
            EndRequest();
        }

        IWorkContextScope IOrchardHost.CreateStandaloneEnvironment(ShellSettings shellSettings) {
            Logger.Debug("Creating standalone environment for tenant {0}", shellSettings.Name);

            MonitorExtensions();
            BuildCurrent();

            var shellContext = CreateShellContext(shellSettings);
            var workContext = shellContext.LifetimeScope.CreateWorkContextScope();
            return new StandaloneEnvironmentWorkContextScopeWrapper(workContext, shellContext);
        }

        /// <summary>
        /// Ensures shells are activated, or re-activated if extensions have changed
        /// </summary>
        IEnumerable<ShellContext> BuildCurrent() {
            // This method gets called:
            // - During initialization of the application
            // - at the beginning of a request, if the system failed to match the HttpContext to a Shell/tenant

            _initializationLock.EnterUpgradeableReadLock();
            try {
                var shellContexts = _shellContextsDictionary.Values;
                // Here we should test whether there are some shells that may have to be activated.
                var shouldInit =
                    // No shell has been activated, so we should activate stuff
                    !shellContexts.Any()
                    // There are fewer active shells than the ones there should be by reading the settings
                    || (shellContexts.Count() < _shellSettingsManager.LoadSettings()
                        .Where(settings => settings.State == TenantState.Running
                            || settings.State == TenantState.Uninitialized || settings.State == TenantState.Initializing)
                        .Count())
                    ;
                if (shouldInit) {
                    _initializationLock.EnterWriteLock();
                    try {
                        SetupExtensions();
                        MonitorExtensions();
                    }
                    finally {
                        _initializationLock.ExitWriteLock();
                    }
                    // here the initialization lock is in UpgradeableRead mode. Downgrade it to
                    // read mode.
                    _initializationLock.EnterReadLock();
                    _initializationLock.ExitUpgradeableReadLock();

                    CreateAndActivateShells();
                }
            }
            finally {
                if (_initializationLock.IsReadLockHeld) {
                    _initializationLock.ExitReadLock();
                }
                if (_initializationLock.IsUpgradeableReadLockHeld) {
                    _initializationLock.ExitUpgradeableReadLock();
                }
            }
            
            return _shellContextsDictionary.Values;
        }

        /// <summary>
        /// Ensures shells are activated, or re-activated if extensions have changed.
        /// This version of the method gives "priority" to a specific shell, passed as
        /// parameter.
        /// </summary> 
        private void BuildCurrent(ShellSettings currentShell) {
            // If the application should reinitialize, it won't go through this call, but rather
            // through its other BuildCurrent().
            if (currentShell == null) {
                BuildCurrent();
            }
            else {
                _initializationLock.EnterUpgradeableReadLock();
                try {
                    // Do we need to activate the requested shell?
                    if (!_shellContextsDictionary.ContainsKey(currentShell.Name)) {
                        _initializationLock.EnterWriteLock();
                        try {
                            SetupExtensions();
                            MonitorExtensions();
                        }
                        finally {
                            _initializationLock.ExitWriteLock();
                        }
                        // here the initialization lock is in UpgradeableRead mode.  Downgrade it to
                        // read mode.
                        _initializationLock.EnterReadLock();
                        _initializationLock.ExitUpgradeableReadLock();
                        _shellActivationLock.RunWithWriteLock(currentShell.Name, () => SingleShellCreationActivation(currentShell));
                    }
                }
                finally {
                    if (_initializationLock.IsReadLockHeld) {
                        _initializationLock.ExitReadLock();
                    }
                    if (_initializationLock.IsUpgradeableReadLockHeld) {
                        _initializationLock.ExitUpgradeableReadLock();
                    }
                }
            }
        }

        void StartUpdatedShells() {
            while (_tenantsToRestart.GetState().Any()) {
                var settings = _tenantsToRestart.GetState().First();
                _tenantsToRestart.GetState().Remove(settings);
                Logger.Debug("Updating shell: " + settings.Name);
                ActivateShell(settings);
            }
        }

        void CreateAndActivateShells() {
            Logger.Information("Start creation of shells");

            // Is there any tenant right now?
            var allSettings = _shellSettingsManager.LoadSettings()
                .Where(settings => settings.State == TenantState.Running || settings.State == TenantState.Uninitialized || settings.State == TenantState.Initializing)
                .ToArray();

            // Load all tenants, and activate their shell.
            if (allSettings.Any()) {
                Parallel.ForEach(allSettings, settings => {
                    // Each shell does its own lock.
                    _shellActivationLock.RunWithWriteLock(settings.Name, () => SingleShellCreationActivation(settings));
                });
            }
            // No settings, run the Setup.
            else {
                var setupContext = CreateSetupContext();
                ActivateShell(setupContext);
            }

            Logger.Information("Done creating shells");
        }

        private void SingleShellCreationActivation(ShellSettings settings) {
            // tries to enter the lock in read mode. If the lock is held in write mode it fails.
            // This case would be if the shells are being disposed: as a result, no shell can be activated
            // while at the same time we are disposing them.
            if (_shellDisposalLock.TryEnterReadLock(0)) {
                try {
                    for (var i = 0; i <= Retries; i++) {

                        // Not the first attempt, wait for a while ...
                        if (DelayRetries && i > 0) {

                            // Wait for i^2 which means 1, 2, 4, 8 ... seconds
                            Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(i, 2)));
                        }

                        try {
                            var context = CreateShellContext(settings);
                            ActivateShell(context);
                            // If everything went well, return to stop the retry loop
                            break;
                        }
                        catch (Exception ex) {
                            if (i == Retries) {
                                Logger.Fatal("A tenant could not be started: {0} after {1} retries.", settings.Name, Retries);
                                return;
                            }
                            else {
                                Logger.Error(ex, "A tenant could not be started: " + settings.Name + " Attempt number: " + i);
                            }
                        }

                    }

                    while (_processingEngine.AreTasksPending()) {
                        Logger.Debug("Processing pending task after activate Shell");
                        _processingEngine.ExecuteNextTask();
                    }
                }
                finally {
                    _shellDisposalLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Starts a Shell and registers its settings in RunningShellTable
        /// </summary>
        private void ActivateShell(ShellContext context) {
            Logger.Debug("Activating context for tenant {0}", context.Settings.Name);
            context.Shell.Activate();
            // Add the computed context to the dictionary.
            _shellContextsDictionary.AddOrUpdate(context.Settings.Name,
                context,
                (k, c) => c = context);

            _runningShellTable.Add(context.Settings);
        }

        /// <summary>
        /// Creates a transient shell for the default tenant's setup.
        /// </summary>
        private ShellContext CreateSetupContext() {
            Logger.Debug("Creating shell context for root setup.");
            return _shellContextFactory.CreateSetupContext(new ShellSettings { Name = ShellSettings.DefaultName });
        }

        /// <summary>
        /// Creates a shell context based on shell settings.
        /// </summary>
        private ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State == TenantState.Uninitialized || settings.State == TenantState.Invalid) {
                Logger.Debug("Creating shell context for tenant {0} setup.", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            Logger.Debug("Creating shell context for tenant {0}.", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        private void SetupExtensions() {
            _extensionLoaderCoordinator.SetupExtensions();
        }

        private void MonitorExtensions() {
            // This is a "fake" cache entry to allow the extension loader coordinator
            // notify us (by resetting _current to "null") when an extension has changed
            // on disk, and we need to reload new/updated extensions.
            _cacheManager.Get("OrchardHost_Extensions", true,
                              ctx => {
                                  _extensionMonitoringCoordinator.MonitorExtensions(ctx.Monitor);
                                  _hostLocalRestart.Monitor(ctx.Monitor);
                                  DisposeShellContext();
                                  return "";
                              });
        }

        /// <summary>
        /// Terminates all active shell contexts, and dispose their scope, forcing
        /// them to be reloaded if necessary.
        /// </summary>
        private void DisposeShellContext() {
            Logger.Information("Disposing active shell contexts");

            // Ideally we would like to have to not configure the lock for recursion, so we would have an
            // easier time preventing deadlocks, as any attempt in that direction would error. Because
            // of that, we check whether we are already holding the lock here, to prevent attempts to
            // retake it.
            if (!_initializationLock.IsWriteLockHeld) {
                _initializationLock.EnterWriteLock();
                _shellDisposalLock.EnterWriteLock();
                try {
                    if (_shellContextsDictionary.Any()) {
                        Parallel.ForEach(_shellContextsDictionary.Values,
                            shellContext => _shellActivationLock.RunWithWriteLock(
                                shellContext.Settings.Name,
                                () => {
                                    shellContext.Shell.Terminate();
                                    shellContext.Dispose();
                                }));
                        _shellContextsDictionary.Clear();
                    }
                }
                finally {
                    _shellDisposalLock.ExitWriteLock();
                    _initializationLock.ExitWriteLock();
                }
            }
        }

        protected virtual void BeginRequest() {
            BlockRequestsDuringSetup();

            ShellSettings currentShellSettings = null;

            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null) {
                currentShellSettings = _runningShellTable.Match(httpContext);
            }
            Logger.Error($"BeginRequest() {httpContext?.Request.Path ?? "null context"} started");

            Action<ShellSettings> ensureInitialized = (currentShell) => {
                // Ensure all shell contexts are loaded, or need to be reloaded if
                // extensions have changed
                MonitorExtensions();
                BuildCurrent(currentShell);
            };

            // BuildCurrent may cause a write lock for the shell, so we don't invoke
            // with a further lock here.
            ensureInitialized(currentShellSettings);

            // StartUpdatedShells can cause a writer shell activation lock so it should run outside the reader lock.
            StartUpdatedShells();

            Logger.Error($"BeginRequest() {httpContext?.Request.Path ?? "null context"} done");
        }

        protected virtual void EndRequest() {
            var httpContext = _httpContextAccessor.Current();
            Logger.Error($"EndRequest() {httpContext?.Request.Path ?? "null context"} started");
            // Synchronously process all pending tasks. It's safe to do this at this point
            // of the pipeline, as the request transaction has been closed, so creating a new
            // environment and transaction for these tasks will behave as expected.)
            while (_processingEngine.AreTasksPending()) {
                Logger.Debug("Processing pending task");
                _processingEngine.ExecuteNextTask();
            }

            StartUpdatedShells();
            Logger.Error($"EndRequest() {httpContext?.Request.Path ?? "null context"} done");
        }

        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings) {
            Logger.Debug("Shell saved: " + settings.Name);

            // if a tenant has been created
            if (settings.State != TenantState.Invalid) {
                if (!_tenantsToRestart.GetState().Any(t => t.Name.Equals(settings.Name))) {
                    Logger.Debug("Adding tenant to restart: " + settings.Name + " " + settings.State);
                    _tenantsToRestart.GetState().Add(settings);
                }
            }
        }

        public void ActivateShell(ShellSettings settings) {
            Logger.Debug("Activating shell: " + settings.Name);

            // look for the associated shell context
            ShellContext shellContext;
            // It's ok if we don't find the context in the dictionary. We'll manage that later.
            _shellContextsDictionary.TryGetValue(settings.Name, out shellContext);

            if (shellContext == null && settings.State == TenantState.Disabled) {
                return;
            }

            // is this is a new tenant ? or is it a tenant waiting for setup ?
            if (shellContext == null || settings.State == TenantState.Uninitialized) {
                _shellActivationLock.RunWithWriteLock(
                    settings.Name,
                    () => {
                        // create the Shell
                        var context = CreateShellContext(settings);

                        // activate the Shell
                        ActivateShell(context);
                    });
            }
            // terminate the shell if the tenant was disabled
            else if (settings.State == TenantState.Disabled) {

                _shellActivationLock.RunWithWriteLock(
                    settings.Name,
                    () => {
                        shellContext.Shell.Terminate();
                        shellContext.Dispose();
                        _runningShellTable.Remove(settings);
                        // remove from dictionary
                        _shellContextsDictionary.TryRemove(settings.Name, out shellContext);
                    });
            }
            // reload the shell as its settings have changed
            else {
                _shellActivationLock.RunWithWriteLock(settings.Name, () => {
                    // dispose previous context if it exists
                    if (shellContext != null) {
                        shellContext.Shell.Terminate();
                        shellContext.Dispose();
                    }

                    var context = _shellContextFactory.CreateShellContext(settings);
                    // Activate and register modified context.
                    context.Shell.Activate();
                    _shellContextsDictionary.AddOrUpdate(settings.Name,
                        context,
                        (k, c) => c = context);

                    _runningShellTable.Update(settings);
                });
            }
        }

        /// <summary>
        /// A feature is enabled/disabled, the tenant needs to be restarted
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {
            if (!_shellContextsDictionary.Values.Any()) {
                return;
            }

            Logger.Debug("Shell changed: " + tenant);

            ShellContext context;
            if(!_shellContextsDictionary.TryGetValue(tenant, out context)) {
                return;
            }

            // don't restart when tenant is in setup
            if (context.Settings.State != TenantState.Running) {
                return;
            }

            // don't flag the tenant if already listed
            if (_tenantsToRestart.GetState().Any(x => x.Name == tenant)) {
                return;
            }

            Logger.Debug("Adding tenant to restart: " + tenant);
            _tenantsToRestart.GetState().Add(context.Settings);
        }

        private void BlockRequestsDuringSetup() {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext.IsBackgroundContext())
                return;

            // Get the requested shell.
            var runningShell = _runningShellTable.Match(httpContext);
            if (runningShell == null)
                return;

            // If the requested shell is currently initializing, return a Service Unavailable HTTP status code.
            if (runningShell.State == TenantState.Initializing) {
                var response = httpContext.Response;
                response.StatusCode = 503;
                response.StatusDescription = "This tenant is currently initializing. Please try again later.";
                response.Write("This tenant is currently initializing. Please try again later.");
            }
        }

        // To be used from CreateStandaloneEnvironment(), also disposes the ShellContext LifetimeScope.
        private class StandaloneEnvironmentWorkContextScopeWrapper : IWorkContextScope {
            private readonly ShellContext _shellContext;
            private readonly IWorkContextScope _workContextScope;

            public WorkContext WorkContext {
                get { return _workContextScope.WorkContext; }
            }

            public StandaloneEnvironmentWorkContextScopeWrapper(IWorkContextScope workContextScope, ShellContext shellContext) {
                _workContextScope = workContextScope;
                _shellContext = shellContext;
            }

            public TService Resolve<TService>() {
                return _workContextScope.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service) {
                return _workContextScope.TryResolve<TService>(out service);
            }

            public void Dispose() {
                _workContextScope.Dispose();
                _shellContext.Dispose();
            }
        }
    }
}
