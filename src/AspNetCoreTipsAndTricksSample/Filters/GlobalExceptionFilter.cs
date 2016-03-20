using System;
using System.Net;

using AspNetCoreTipsAndTricksSample.Exceptions;
using AspNetCoreTipsAndTricksSample.Responses;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AspNetCoreTipsAndTricksSample.Filters
{
    /// <summary>
    /// This represents the filter entity for global exceptions.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {
        private readonly ILogger _logger;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionFilter"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
        public GlobalExceptionFilter(ILoggerFactory logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._logger = logger.CreateLogger("Global Exception Filter");
        }

        /// <summary>
        /// Performs while an exception arises.
        /// </summary>
        /// <param name="context"><see cref="ExceptionContext"/> instance.</param>
        public void OnException(ExceptionContext context)
        {
            var response = new ErrorResponse() { Message = context.Exception.Message };
#if DEBUG
            response.StackTrace = context.Exception.StackTrace;
#endif
            context.Result = new ObjectResult(response)
                                 {
                                     StatusCode = GetHttpStatusCode(context.Exception),
                                     DeclaredType = typeof(ErrorResponse)
                                 };

            this._logger.LogError("GlobalExceptionFilter", context.Exception);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }

        private static int GetHttpStatusCode(Exception ex)
        {
            if (ex is HttpResponseException)
            {
                return (int)(ex as HttpResponseException).HttpStatusCode;
            }

            return (int)HttpStatusCode.InternalServerError;
        }
    }
}