using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace WebApiDemo.Api.ErrorHandling
{
    [ExcludeFromCodeCoverage]
    internal class HttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ContentType { get; set; } = @"text/plain";

        public HttpStatusCodeException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message, Exception inner) : base(message, inner)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, JObject errorObject) : this(statusCode, errorObject.ToString())
        {
            ContentType = @"application/json";
        }
    }
}
