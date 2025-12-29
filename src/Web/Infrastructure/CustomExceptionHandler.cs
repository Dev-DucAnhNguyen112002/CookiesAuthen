using CookiesAuthen.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidationException = FluentValidation.ValidationException;
namespace CookiesAuthen.Web.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new()
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(FluentValidationException), HandleFluentValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
                { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (_exceptionHandlers.ContainsKey(exceptionType))
        {
            await _exceptionHandlers[exceptionType].Invoke(httpContext, exception);
            return true;
        }

        return false;
    }

    //private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    //{
    //    var exception = (ValidationException)ex;

    //    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

    //    await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
    //    {
    //        Status = StatusCodes.Status400BadRequest,
    //        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    //    });
    //}

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        });
    }

    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        });
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        });
    }
    // 1. Xử lý lỗi từ FluentValidation (Thư viện)
    private async Task HandleFluentValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (FluentValidation.ValidationException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        // Chuyển đổi lỗi từ Fluent sang Dictionary
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failure => failure.Key, failure => failure.ToArray());

        // FIX NULL & EMPTY: Nếu không có lỗi chi tiết, lấy Message làm lỗi chung
        if (!errors.Any() && !string.IsNullOrWhiteSpace(exception.Message))
        {
            errors.Add("Error", new[] { exception.Message });
        }

        // errors ở đây chắc chắn không null vì được khởi tạo từ Linq hoặc Dictionary mới
        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Error"
        });
    }

    // 2. Xử lý lỗi từ Application Validation (Của bạn)
    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        IDictionary<string, string[]> errors = exception.Errors ?? new Dictionary<string, string[]>();
        // Logic fallback: Nếu danh sách rỗng, lấy Message chung
        if (!errors.Any() && !string.IsNullOrWhiteSpace(exception.Message))
        {
            // Phải khởi tạo lại Dictionary mới để add được (tránh lỗi Array cố định)
            errors = new Dictionary<string, string[]>
            {
                { "Error", new[] { exception.Message } }
            };
        }

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        });
    }
}
