using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Routing
{
    public static class RoutingExtensions
    {
        public static RouteData ParseRouteData(this RouteCollection routes, string url, HttpRequestBase request)
        {
            Uri uri = request.AbsoluteUri(url);
            return routes.ParseRouteData(uri, request.ApplicationPath);
        }

        public static RouteData ParseRouteData(this RouteCollection routes, Uri uri, string applicationPath)
        {
            return routes.GetRouteData(new InternalHttpContext(uri, applicationPath));
        }

        private class InternalHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public InternalHttpContext(Uri uri, string applicationPath)
            {
                _request = new InternalRequestContext(uri, applicationPath);
            }

            public override HttpRequestBase Request { get { return _request; } }
        }

        private class InternalRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            public InternalRequestContext(Uri uri, string applicationPath)
            {
                _pathInfo = uri.Query;

                if (String.IsNullOrEmpty(applicationPath) || !uri.AbsolutePath.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
                    _appRelativePath = uri.AbsolutePath.Substring(applicationPath.Length);
                else
                    _appRelativePath = uri.AbsolutePath;
            }

            public override string AppRelativeCurrentExecutionFilePath { get { return String.Concat("~", _appRelativePath); } }
            public override string PathInfo { get { return _pathInfo; } }
        }

    }
}