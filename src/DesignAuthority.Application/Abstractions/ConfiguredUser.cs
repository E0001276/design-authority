namespace DesignAuthority.Application.Abstractions;

public interface IConfiguredUserContext
{
    int UserId { get; }
}

public sealed record ServiceResult<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors)
{
    public bool Succeeded => Errors.Count == 0;

    public static ServiceResult<T> Success(T value) => new(value, new Dictionary<string, string[]>());

    public static ServiceResult<T> Failure(string field, string error) =>
        new(default, new Dictionary<string, string[]> { [field] = [error] });
}
