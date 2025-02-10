using Microsoft.AspNetCore.Http;
using System;

public static class HttpRequestExtensions
{
    private const string XmlHttpRequest = "XMLHttpRequest";

    public static bool IsAjaxRequest(this HttpRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return request.Headers["X-Requested-With"] == XmlHttpRequest;
    }
}
