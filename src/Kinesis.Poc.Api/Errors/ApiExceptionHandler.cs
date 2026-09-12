using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Kinesis.Poc.Api.Errors;

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Ocurrió un error interno al procesar la solicitud.",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        // La respuesta no incluye excepciones, detalles de SQL ni credenciales.
        if (!await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        }))
        {
            await httpContext.Response.WriteAsJsonAsync(problem,
                options: (System.Text.Json.JsonSerializerOptions?)null,
                contentType: "application/problem+json", cancellationToken: cancellationToken);
        }

        return true;
    }
}
