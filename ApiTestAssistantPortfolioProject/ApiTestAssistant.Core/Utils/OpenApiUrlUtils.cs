using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestAssistant.Core.Utils
{
    public static class OpenApiUrlUtils
    {
        public static string? NormalizeOpenApiUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            if (url.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
                return url;

            if (url.EndsWith("index.html", StringComparison.OrdinalIgnoreCase))
                return url.Replace("index.html", "v1/swagger.json");

            if (url.EndsWith("/swagger", StringComparison.OrdinalIgnoreCase))
                return url + "/v1/swagger.json";

            if (url.EndsWith("/swagger/", StringComparison.OrdinalIgnoreCase))
                return url + "v1/swagger.json";

            return url;
        }
    }
}
