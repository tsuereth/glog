using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;

namespace GlogGenerator
{
    public class HostLocalSite
    {
        public static void Host(
            ILogger logger,
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
                                var responseTimer = Stopwatch.StartNew();
                                content.WriteHttpListenerResponse(site, ref response);
                                responseTimer.Stop();
                                logger.LogInformation("Wrote {ContentPath} in {ResponseTimeMs} ms",
                                    pathCheck,
                                    responseTimer.ElapsedMilliseconds);
                                break;
                            }
                        }

                        if (!pathFound)
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            logger.LogWarning("Didn't find content matching {RequestPath}",
                                requestPath);
                        }
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        logger.LogWarning("Request path {RequestPath} didn't start with expected prefix {PathPrefix}",
                            requestPath,
                            pathPrefix);
                    }

                    response.Close();
                }
            }
        }
    }
}
