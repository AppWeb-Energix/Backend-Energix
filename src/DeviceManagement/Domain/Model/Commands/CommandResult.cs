namespace Energix.API.DeviceManagement.Domain.Model.Commands;

/// <summary>
/// Generic result for commands with data
/// </summary>
public record CommandResult<T>(
    bool Success,
    T? Data = default,
    string? ErrorMessage = null
)
{
    public static CommandResult<T> Ok(T data) => new(true, data);
    public static CommandResult<T> Fail(string error) => new(false, default, error);
}

/// <summary>
/// Simple result for commands without data
/// </summary>
public record CommandResult(
    bool Success,
    string? ErrorMessage = null
)
{
    public static CommandResult Ok() => new(true);
    public static CommandResult Fail(string error) => new(false, error);
}