﻿using Castle.Windsor;
using Serilog;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Windsor;
using Nancy.ViewEngines;

namespace OpenHomeServer.Server.Web
{
    public class NancyBootstrapper : WindsorNancyBootstrapper
    {
        private static IWindsorContainer _container;

        public NancyBootstrapper()
        {            
            ResourceViewLocationProvider.RootNamespaces.Add(GetType().Assembly, "OpenHomeServer.Server");            
        }        

        public static void SetApplicationContainer(IWindsorContainer container)
        {
            _container = container;
        }

        protected override Castle.Windsor.IWindsorContainer GetApplicationContainer()
        {
            return _container;
        }        

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(x => {
                    x.ViewLocationProvider = typeof(ResourceViewLocationProvider);             
                });
            }
        }

        protected override void ConfigureConventions(Nancy.Conventions.NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.Add(
                Nancy.Embedded.Conventions.EmbeddedStaticContentConventionBuilder.AddDirectory("Assets", this.GetType().Assembly, "Web\\Assets")
            );
            conventions.ViewLocationConventions.Add((viewName, model, viewLocationContext) =>
            {
                return string.Concat("Plugins/", viewLocationContext.ModuleName, "/", viewName);
            });
            conventions.ViewLocationConventions.Add((viewName, model, viewLocationContext) =>
            {
                return string.Concat("Web/Views/", viewName);
            });
        }

        protected override void ApplicationStartup(IWindsorContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            var logger = Log.Logger; //container.Resolve<Common.Logging.ILog>();

            pipelines.OnError += (ctx, ex) =>
            {
                logger.Error("", ex);
                return null;
            };
        }

#if DEBUG
        protected override Nancy.Diagnostics.DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get
            {
                return new Nancy.Diagnostics.DiagnosticsConfiguration { Password="test"};
            }
        }
#endif
    }
}
