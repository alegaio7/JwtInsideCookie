using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace UI
{
    /// <summary>
    /// This exception base class is used to avoid a catch-all pattern in try-catches, so a custom status code/message can be sent 
    /// to the client instead of a InternalServerError exception.
    /// </summary>
    public class UIGenericHttpException : Exception
    {
        public UIGenericHttpException(HttpStatusCode code)
        {
            StatusCode = code;
        }

        public UIGenericHttpException(HttpStatusCode code, string message) : base(message)
        {
            StatusCode = code;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
