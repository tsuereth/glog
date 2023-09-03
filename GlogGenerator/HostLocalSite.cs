using System.Collections.Generic;
using System.Net;
using GlogGenerator.RenderState;

namespace GlogGenerator
{
    public static class HostLocalSite
    {
        public static void Host(
            SiteState site,
            string hostOrigin,
            string pathPrefix)
        {
            using (var listener = new HttpListener())
            {
                if (!hostOrigin.EndsWith('/'))
                {
                    hostOrigin += '/';
                }

                listener.Prefixes.Add(hostOrigin);

                listener.Start();

                if (!string.IsNullOrEmpty(pathPrefix) && !pathPrefix.StartsWith('/'))
                {
                    pathPrefix = $"/{pathPrefix}";
                }

                while (true)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    var requestPath = request.Url.AbsolutePath;

                    if (requestPath.StartsWith(pathPrefix, System.StringComparison.OrdinalIgnoreCase))
                    {
                        var requestPathRelative = requestPath.Substring(pathPrefix.Length);

                        // Emulate some nginx and/or common web server behaviors:
                        // - Check for a route exactly matching PATH
                        // - Check for a route matching PATH + '/'
                        // - Check for a route matching PATH [+ '/'] + "index.html"
                        var pathChecks = new List<string>()
                        {
                            requestPathRelative,
                        };

                        if (!string.IsNullOrEmpty(requestPathRelative) && !requestPathRelative.EndsWith('/'))
                        {
                            requestPathRelative += '/';
                            pathChecks.Add(requestPathRelative);
                        }

                        pathChecks.Add(requestPathRelative + "index.html");

                        var pathFound = false;
                        foreach (var pathCheck in pathChecks)
                        {
                            if (site.ContentRoutes.TryGetValue(pathCheck, out var content))
                            {
                                pathFound = true;
                                content.WriteHttpListenerResponse(site, ref response);
                                break;
                            }
                        }

                        if (!pathFound)
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }

                    response.Close();
                }
            }
        }
    }
}
