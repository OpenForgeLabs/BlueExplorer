using FluentResults;

namespace Commons.Results;

public sealed class HandledFail : Error
{
    public IReadOnlyList<string> ReasonsList { get; }

    public HandledFail(string message, IEnumerable<string> reasons)
        : base(message)
    {
        ReasonsList = reasons?.ToList() ?? new List<string>();
    }
}
