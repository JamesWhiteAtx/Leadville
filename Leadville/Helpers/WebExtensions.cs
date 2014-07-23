using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web
{
    public static class RequestExtensions
    {
        public static Uri AbsoluteUri(this HttpRequestBase request, string url)
        {
            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
            //If the URI is not already absolute, rebuild it based on the current request.
            if (!uri.IsAbsoluteUri)
            {
                UriBuilder builder = new UriBuilder(request.Url.GetLeftPart(UriPartial.Authority));
                //All that needs to change is the path portion.
                builder.Path = VirtualPathUtility.ToAbsolute(url);

                uri = builder.Uri;
            }
            return uri;
        }

        //    public static bool IsUrlLocalToHost(this HttpRequestBase request, string url)
        //    {
        //        if (String.IsNullOrEmpty(url))
        //        {
        //            return false;
        //        }

        //        Uri absoluteUri;
        //        if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
        //        {
        //            return String.Equals(request.Url.Host, absoluteUri.Host,
        //                        StringComparison.OrdinalIgnoreCase);
        //        }
        //        else
        //        {
        //            bool isLocal = !url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
        //                && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
        //                && Uri.IsWellFormedUriString(url, UriKind.Relative);
        //            return isLocal;
        //        }
        //    }
    }

}