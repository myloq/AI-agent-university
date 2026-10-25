namespace AiAgentUniversity.Cli;

public class ConsoleUI
{
    public void PrintWelcome(string dataPath)
    {
        Console.WriteLine("=== AI Agent: учёт домашних запасов ===");
        Console.WriteLine($"Файл данных: {dataPath}");
        Console.WriteLine("Введите `help` для списка команд или `exit` для выхода.\n");
    }

    public void PrintResult(CommandResult result)
    {
        foreach (var line in result.Lines) Console.WriteLine(line);
        Console.WriteLine();
    }
}
