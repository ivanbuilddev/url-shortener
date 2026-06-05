using System.Net;

namespace UrlShortener.DTOs;

public class Result<T>
{
    public T? Value {get; set;}
    public HttpStatusCode Status {get; set;}
    public string? ErrorMessage {get; set;}

    private Result() {}

    public bool IsAnyError() => Value == null;

    public static Result<T> Success(T? value) => new () { Value = value, Status = HttpStatusCode.OK };
    public static Result<T> Unauthorized(string message = "Unauthorized") => new () { ErrorMessage = message, Status = HttpStatusCode.Unauthorized };
    public static Result<T> NotFound(string message = "Not found") => new () { ErrorMessage = message, Status = HttpStatusCode.NotFound };
    public static Result<T> Gone(string message = "Resource no longer available") => new () { ErrorMessage = message, Status = HttpStatusCode.Gone };
    public static Result<T> Conflict(string message = "Conflict") => new () { ErrorMessage = message, Status = HttpStatusCode.Conflict };
    public static Result<T> Failure(string message = "Server Error") => new () { ErrorMessage = message, Status = HttpStatusCode.InternalServerError };
    public static Result<T> Forbidden(string message = "Forbidden") => new () { ErrorMessage = message, Status = HttpStatusCode.Forbidden };

}