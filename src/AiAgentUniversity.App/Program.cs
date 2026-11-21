using System.Text;
using AiAgentUniversity.App.Cli;
using AiAgentUniversity.Data.Storage;
using AiAgentUniversity.Domain.Interfaces;
using AiAgentUniversity.Domain.Services;

Console.OutputEncoding = Encoding.UTF8;

var dataPath = Path.Combine(Environment.CurrentDirectory, "data.json");

IStorage storage = new JsonStorage(dataPath);
var service = new ProductService(storage);
var processor = new CommandProcessor(service);
var ui = new ConsoleUI();

ui.PrintWelcome(dataPath);

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line is null) { ui.PrintResult(CommandResult.Exit("До свидания!")); break; }
    if (string.IsNullOrWhiteSpace(line)) continue;

    var command = CommandParser.Parse(line);
    var result = processor.Execute(command);
    ui.PrintResult(result);
    if (result.ShouldExit) break;
}
