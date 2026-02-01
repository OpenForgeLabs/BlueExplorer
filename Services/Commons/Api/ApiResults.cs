using Commons.Results;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Commons.Api;

public static class ApiResults
{
    public static IResult Ok(string message = "")
        => Microsoft.AspNetCore.Http.Results.Ok(ApiResponse.Success(null, message));

    public static IResult Ok<T>(T data, string message = "")
        => Microsoft.AspNetCore.Http.Results.Ok(ApiResponse<T>.Success(data, message));

    public static IResult BadRequest(string message, IEnumerable<string>? reasons = null)
        => Microsoft.AspNetCore.Http.Results.BadRequest(ApiResponse.Fail(message, reasons));

    public static IResult NotFound(string message = "Not found")
        => Microsoft.AspNetCore.Http.Results.NotFound(ApiResponse.Fail(message, new[] { message }));

    public static IResult Conflict(string message)
        => Microsoft.AspNetCore.Http.Results.Conflict(ApiResponse.Fail(message, new[] { message }));

    public static IResult FromResult(Result result, string successMessage = "")
    {
        if (result.IsSuccess)
            return Ok(successMessage);

        ApiResponse fail = FailFrom(result);
        return Microsoft.AspNetCore.Http.Results.BadRequest(fail);
    }

    public static IResult FromResult<T>(Result<T> result, string successMessage = "")
    {
        if (result.IsSuccess)
            return Ok(result.Value, successMessage);

        ApiResponse fail = FailFrom(result);
        return Microsoft.AspNetCore.Http.Results.BadRequest(fail);
    }

    private static ApiResponse FailFrom(ResultBase result)
    {
        List<string> reasons = new();
        foreach (IReason reason in result.Reasons)
        {
            if (reason is HandledFail handled)
            {
                reasons.AddRange(handled.ReasonsList);
                continue;
            }

            reasons.Add(reason.Message);
        }

        string message = result.Errors.FirstOrDefault()?.Message
                         ?? result.Reasons.FirstOrDefault()?.Message
                         ?? "Request failed";

        return ApiResponse.Fail(message, reasons);
    }
}
