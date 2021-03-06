﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NDjango.Interfaces;
using System.Web;
using System.IO;
using System.Web.Routing;

namespace NDjango.ASPMVC
{
    public class DjangoViewEngine : VirtualPathProviderViewEngine, ITemplateLoader
    {
        public DjangoViewEngine()
        {
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.django", "~/Views/Shared/{0}.django" };
            base.AreaViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.django", "~/Areas/{2}/Views/Shared/{0}.django" };
            base.PartialViewLocationFormats = base.ViewLocationFormats;
            base.AreaPartialViewLocationFormats = base.AreaViewLocationFormats;
            server = HttpContext.Current.Server;
            manager_provider = new NDjango.TemplateManagerProvider().WithLoader(this).WithTag("url", new AspMvcUrlTag());
        }

        public DjangoViewEngine(Func<TemplateManagerProvider, TemplateManagerProvider> setup)
            : this()
        {
            manager_provider = setup(manager_provider).WithLoader(this);
            server = HttpContext.Current.Server;
        }

        HttpServerUtility server;
        NDjango.TemplateManagerProvider manager_provider;
        System.Reflection.PropertyInfo manager_property;

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return CreateView(controllerContext, partialPath, null);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            // ptentially this can cause a racing condition when more than one thread will rush to set the manager_property value
            // I think it is still ok because all of them will get back the same value and if it is assigned more than once it should be 
            // no problem
            if (manager_property == null)
            {
                manager_property = controllerContext.HttpContext.ApplicationInstance.GetType().GetProperty("DjangoTemplateManager");
                if (manager_property == null || !manager_property.CanWrite || !manager_property.CanRead || manager_property.PropertyType != typeof(ITemplateManager))
                    throw new ApplicationException("Missing or invalid TemplateManager property in Global.asax. The required format is\n        public NDjango.Interfaces.ITemplateManager DjangoTemplateManager { get; set; }");
            }
            var manager = (ITemplateManager) manager_property.GetValue(controllerContext.HttpContext.ApplicationInstance, new object[] { });
            if (manager == null)
            {
                manager = manager_provider.GetNewManager();
                manager_property.SetValue(controllerContext.HttpContext.ApplicationInstance, manager, new object[] { });
            }

            return new DjangoView(manager, viewPath);
        }

        private string MapPath(string virtual_path)
        {
            return server.MapPath(virtual_path);
        }

        #region ITemplateLoader Members

        public System.IO.TextReader GetTemplate(string path)
        {
            return new StreamReader(MapPath(path));
        }

        public bool IsUpdated(string path, DateTime timestamp)
        {
            return File.GetLastWriteTime(MapPath(path)) > timestamp;
        }

        #endregion
    }

    
}
