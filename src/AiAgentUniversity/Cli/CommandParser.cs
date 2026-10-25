using System.Text;

namespace AiAgentUniversity.Cli;

public static class CommandParser
{
    public static ParsedCommand Parse(string line)
    {
        var tokens = Tokenize(line);
        if (tokens.Count == 0) return new ParsedCommand(string.Empty, Array.Empty<string>());
        return new ParsedCommand(tokens[0].ToLowerInvariant(), tokens.Skip(1).ToList());
    }

    private static List<string> Tokenize(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"') { inQuotes = !inQuotes; continue; }
            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (current.Length > 0) { result.Add(current.ToString()); current.Clear(); }
                continue;
            }
            current.Append(ch);
        }
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }
}
