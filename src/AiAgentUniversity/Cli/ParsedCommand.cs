namespace AiAgentUniversity.Cli;

public record ParsedCommand(string Name, IReadOnlyList<string> Args);