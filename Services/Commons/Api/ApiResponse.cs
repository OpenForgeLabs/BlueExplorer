namespace Commons.Api;

public sealed class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Reasons { get; set; } = [];
    public object? Data { get; set; }

    public static ApiResponse Success(object? data = null, string message = "")
    {
        return new ApiResponse
        {
            IsSuccess = true,
            Message = message,
            Reasons = [],
            Data = data
        };
    }

    public static ApiResponse Fail(string message, IEnumerable<string>? reasons = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Message = message,
            Reasons = reasons?.ToList() ?? new List<string> { message },
            Data = null
        };
    }
}

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Reasons { get; set; } = [];
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T? data = default, string message = "")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Reasons = [],
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? reasons = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Reasons = reasons?.ToList() ?? new List<string> { message },
            Data = default
        };
    }
}
