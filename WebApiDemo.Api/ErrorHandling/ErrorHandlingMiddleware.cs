using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

namespace WebApiDemo.Api.ErrorHandling
{
    [ExcludeFromCodeCoverage]
    internal class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);

                if (context.Response.HasStarted)
                {
                    _logger.Warning("The response has already started, the error handling middleware will not be executed.");
                }
                else
                {
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string result;
            context.Response.ContentType = "application/json";

            _logger.Error(exception, "Internal error occured: " + exception.Message);

            if (exception is HttpStatusCodeException)
            {
                HttpStatusCodeException httpStatusCodeException = exception as HttpStatusCodeException;
                context.Response.StatusCode = (int)httpStatusCodeException.StatusCode;
                result = new ErrorDetails
                {
                    Message = exception.Message,
                    StatusCode = context.Response.StatusCode
                }.ToString();
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                result = new ErrorDetails
                {
                    Message = "Internal Server Error - please try again later",
                    StatusCode = context.Response.StatusCode
                }.ToString();
            }

            return context.Response.WriteAsync(result);
        }
    }
}
