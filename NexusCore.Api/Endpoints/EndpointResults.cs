using NexusCore.SharedKernel.Results;

namespace NexusCore.Api.Endpoints;

public static class EndpointResults
{
    public static IResult ToApiResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return ToProblem(result.Error);
    }

    public static IResult ToApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error)
    {
        var statusCode = error.Code switch
        {
            "validation.error" => StatusCodes.Status400BadRequest,
            "not_found" => StatusCodes.Status404NotFound,
            "conflict" => StatusCodes.Status409Conflict,
            "unauthorized" => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(error.Message, statusCode: statusCode, title: error.Code);
    }
}
