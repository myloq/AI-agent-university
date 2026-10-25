using System.Globalization;
using AiAgentUniversity.Services;

namespace AiAgentUniversity.Cli;

public class CommandProcessor
{
    private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy" };
    private readonly ProductService _service;

    public CommandProcessor(ProductService service) => _service = service;

    public CommandResult Execute(ParsedCommand cmd)
    {
        try
        {
            return cmd.Name switch
            {
                "" => CommandResult.Ok(),
                "help" => Help(),
                "exit" or "quit" => CommandResult.Exit("Завершение работы. До свидания!"),
                "add" => Add(cmd.Args),
                "list" => List(),
                "expiring" => Expiring(cmd.Args),
                "category" => Category(cmd.Args),
                "find" => Find(cmd.Args),
                "consume" => Consume(cmd.Args),
                _ => CommandResult.Error($"неизвестная команда «{cmd.Name}». Введите `help`."),
            };
        }
        catch (Exception ex)
        {
            return CommandResult.Error($"непредвиденная ошибка: {ex.Message}");
        }
    }

    private static CommandResult Help() => CommandResult.Ok(
        "Доступные команды:",
        "  add <название> <кол-во> <ед> <категория> <дата>  — добавить продукт",
        "  list                                             — показать все запасы",
        "  expiring <дни>                                   — что истекает в ближайшие N дней",
        "  category <категория>                             — продукты выбранной категории",
        "  find <подстрока>                                 — поиск по названию",
        "  consume <id> <кол-во>                            — списать",
        "  help                                             — эта справка",
        "  exit | quit                                      — завершить работу");

    private CommandResult Add(IReadOnlyList<string> args)
    {
        if (args.Count != 5) return CommandResult.Error("использование: add <название> <кол-во> <ед> <категория> <дата>");
        if (!TryParseQuantity(args[1], out var qty) || qty <= 0) return CommandResult.Error($"некорректное количество: «{args[1]}».");
        if (!DateTime.TryParseExact(args[4], DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiry)) return CommandResult.Error($"некорректная дата: «{args[4]}».");

        var product = _service.Add(args[0], qty, args[2], args[3], expiry);
        return CommandResult.Ok($"Добавлено: #{product.Id} «{product.Name}» — {product.Quantity} {product.Unit}, срок: {product.ExpiryDate:dd.MM.yyyy}.");
    }

    private CommandResult List()
    {
        var items = _service.All();
        if (items.Count == 0) return CommandResult.Ok("Запасы пусты.");
        var lines = new List<string> { "Текущие запасы:" };
        foreach (var p in items.OrderBy(p => p.ExpiryDate))
            lines.Add($"  #{p.Id} «{p.Name}» — {p.Quantity} {p.Unit}, категория: {p.Category}, срок: {p.ExpiryDate:dd.MM.yyyy} ({DescribeDays(p.DaysLeft())})");
        return CommandResult.Ok(lines.ToArray());
    }

    private CommandResult Expiring(IReadOnlyList<string> args)
    {
        if (args.Count != 1) return CommandResult.Error("использование: expiring <дни>");
        if (!int.TryParse(args[0], out var days) || days < 0) return CommandResult.Error($"некорректное число дней: «{args[0]}».");

        var items = _service.ExpiringWithin(days);
        if (items.Count == 0) return CommandResult.Ok($"В ближайшие {days} дн. ничего не истекает.");

        var lines = new List<string> { $"Истекает в ближайшие {days} дн.:" };
        foreach (var p in items) lines.Add($"  #{p.Id} «{p.Name}» — {p.Quantity} {p.Unit} ({DescribeDays(p.DaysLeft())})");
        return CommandResult.Ok(lines.ToArray());
    }

    private CommandResult Category(IReadOnlyList<string> args)
    {
        if (args.Count != 1) return CommandResult.Error("использование: category <категория>");
        var items = _service.ByCategory(args[0]);
        if (items.Count == 0) return CommandResult.Ok($"В категории «{args[0]}» продуктов нет.");
        var lines = new List<string> { $"Продукты в категории «{args[0]}»:" };
        foreach (var p in items) lines.Add($"  #{p.Id} «{p.Name}» — {p.Quantity} {p.Unit}");
        return CommandResult.Ok(lines.ToArray());
    }

    private CommandResult Find(IReadOnlyList<string> args)
    {
        if (args.Count != 1) return CommandResult.Error("использование: find <подстрока>");
        var items = _service.Search(args[0]);
        if (items.Count == 0) return CommandResult.Ok($"По запросу «{args[0]}» ничего не найдено.");
        var lines = new List<string> { $"Найдено по запросу «{args[0]}»:" };
        foreach (var p in items) lines.Add($"  #{p.Id} «{p.Name}» — {p.Quantity} {p.Unit}");
        return CommandResult.Ok(lines.ToArray());
    }

    private CommandResult Consume(IReadOnlyList<string> args)
    {
        if (args.Count != 2) return CommandResult.Error("использование: consume <id> <кол-во>");
        if (!int.TryParse(args[0], out var id) || id <= 0) return CommandResult.Error($"некорректный id: «{args[0]}».");
        if (!TryParseQuantity(args[1], out var amount) || amount <= 0) return CommandResult.Error($"некорректное количество: «{args[1]}».");

        if (!_service.Consume(id, amount, out var message)) return CommandResult.Error(message);
        return CommandResult.Ok(message);
    }

    private static bool TryParseQuantity(string raw, out double value) =>
        double.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static string DescribeDays(int days) => days switch
    {
        < 0 => "просрочено",
        0 => "истекает сегодня",
        1 => "истекает завтра",
        _ => $"осталось {days} дн.",
    };
}
