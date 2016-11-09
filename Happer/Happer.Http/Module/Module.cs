﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http.Routing;

namespace Happer.Http
{
    public abstract class Module
    {
        private readonly List<Route> _routes = new List<Route>();

        protected Module()
            : this(string.Empty)
        {
        }

        protected Module(string modulePath)
        {
            this.ModulePath = modulePath;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<Route> Routes
        {
            get { return _routes.AsReadOnly(); }
        }

        public string ModulePath { get; protected set; }

        public string GetModuleName()
        {
            var typeName = this.GetType().Name;

            var offset = typeName.LastIndexOf("Module", StringComparison.Ordinal);

            if (offset <= 0)
            {
                return typeName;
            }

            return typeName.Substring(0, offset);
        }

        public Context Context { get; set; }

        public Request Request
        {
            get { return this.Context.Request; }
            set { this.Context.Request = value; }
        }

        public ResponseFormatter Response { get; set; }

        public RouteBuilder Delete
        {
            get { return new RouteBuilder("DELETE", this); }
        }

        public RouteBuilder Get
        {
            get { return new RouteBuilder("GET", this); }
        }

        public RouteBuilder Head
        {
            get { return new RouteBuilder("HEAD", this); }
        }

        public RouteBuilder Options
        {
            get { return new RouteBuilder("OPTIONS", this); }
        }

        public RouteBuilder Patch
        {
            get { return new RouteBuilder("PATCH", this); }
        }

        public RouteBuilder Post
        {
            get { return new RouteBuilder("POST", this); }
        }

        public RouteBuilder Put
        {
            get { return new RouteBuilder("PUT", this); }
        }

        public class RouteBuilder
        {
            private readonly string _method;
            private readonly Module _parentModule;

            public RouteBuilder(string method, Module parentModule)
            {
                _method = method;
                _parentModule = parentModule;
            }

            public Func<dynamic, dynamic> this[string path]
            {
                set { this.AddRoute(string.Empty, path, null, value); }
            }

            public Func<dynamic, dynamic> this[string path, Func<Context, bool> condition]
            {
                set { this.AddRoute(string.Empty, path, condition, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string path, bool runAsync]
            {
                set { this.AddRoute(string.Empty, path, null, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string path, Func<Context, bool> condition, bool runAsync]
            {
                set { this.AddRoute(string.Empty, path, condition, value); }
            }

            public Func<dynamic, dynamic> this[string name, string path]
            {
                set { this.AddRoute(name, path, null, value); }
            }

            public Func<dynamic, dynamic> this[string name, string path, Func<Context, bool> condition]
            {
                set { this.AddRoute(name, path, condition, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string name, string path, bool runAsync]
            {
                set { this.AddRoute(name, path, null, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string name, string path, Func<Context, bool> condition, bool runAsync]
            {
                set { this.AddRoute(name, path, condition, value); }
            }

            protected void AddRoute(string name, string path, Func<Context, bool> condition, Func<dynamic, dynamic> value)
            {
                var fullPath = GetFullPath(path);

                _parentModule._routes.Add(Route.FromSync(name, _method, fullPath, condition, value));
            }

            protected void AddRoute(string name, string path, Func<Context, bool> condition, Func<dynamic, CancellationToken, Task<dynamic>> value)
            {
                var fullPath = GetFullPath(path);

                _parentModule._routes.Add(new Route(name, _method, fullPath, condition, value));
            }

            private string GetFullPath(string path)
            {
                var relativePath = (path ?? string.Empty).Trim('/');
                var parentPath = (_parentModule.ModulePath ?? string.Empty).Trim('/');

                if (string.IsNullOrEmpty(parentPath))
                {
                    return string.Concat("/", relativePath);
                }

                if (string.IsNullOrEmpty(relativePath))
                {
                    return string.Concat("/", parentPath);
                }

                return string.Concat("/", parentPath, "/", relativePath);
            }
        }
    }
}
