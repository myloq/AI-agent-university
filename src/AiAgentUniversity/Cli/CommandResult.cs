namespace AiAgentUniversity.Cli;

public record CommandResult(bool ShouldExit, IReadOnlyList<string> Lines)
{
    public static CommandResult Ok(params string[] lines) => new(false, lines);
    public static CommandResult Exit(params string[] lines) => new(true, lines);
    public static CommandResult Error(string message) => new(false, new[] { $"[ошибка] {message}" });
}