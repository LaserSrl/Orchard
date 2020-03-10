using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Module = Autofac.Module;

namespace Orchard.Localization {
    public class LocalizationModule : Module {
        private readonly ConcurrentDictionary<string, Localizer> _localizerCache;//TODO IEnumerable

        public LocalizationModule() {
            _localizerCache = new ConcurrentDictionary<string, Localizer>();
        }

        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<Text>().As<IText>().InstancePerDependency();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            var userProperty = FindUserProperty(registration.Activator.LimitType);
            if (userProperty != null) {
                List<string> scopes = new List<string>();
                var type = registration.Activator.LimitType;
                while (type != typeof(System.Object)) {//TODO: System.*
                    scopes.Add(type.FullName);
                    type = type.BaseType;
                }

                //if (scopes.Count == 1) {
                //    registration.Activated += (sender, e) => {
                //        if (e.Instance.GetType().FullName != scopes.First()) {
                //            return;
                //        }
                //        var localizer = _localizerCache.GetOrAdd(scopes.First(), key => LocalizationUtilities.Resolve(e.Context, scopes.First()));
                //        userProperty.SetValue(e.Instance, localizer, null);
                //    };
                //}
                //else {
                    registration.Activated += (sender, e) => {
                        //if (e.Instance.GetType().FullName != scopes) { //todo: verificare correttezza controllo
                        //    return;
                        //}
                        var localizer = _localizerCache.GetOrAdd(scopes.First(), key => LocalizationUtilities.Resolve(e.Context, scopes));
                        userProperty.SetValue(e.Instance, localizer, null);
                    };
                //}
            }
        }

        private static PropertyInfo FindUserProperty(Type type) {
            return type.GetProperty("T", typeof(Localizer));
        }
    }
}
