using System.Net;
using System.Text.Json;

namespace Todoify.Api.Middleware
{
    /// <summary>
    /// Middleware that catches unhandled exceptions and returns a structured RFC 7807 problem details response.
    /// Also gracefully handles client disconnection via <see cref="OperationCanceledException"/>.
    /// </summary>
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Invokes the middleware, passing the request to the next delegate and catching any unhandled exceptions.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Client disconnected — no need to log or respond
                context.Response.StatusCode = 499; // Client Closed Request
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7807",
                    title = "An unexpected error occurred.",
                    status = 500,
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem, s_jsonOptions));
            }
        }
    }
}
