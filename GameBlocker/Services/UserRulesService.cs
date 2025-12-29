// Services/UserRulesService.cs

using System.Text.Json;

public class UserRulesService
{
    // 1. Define the file path
    // Use AppContext.BaseDirectory to ensure it lives next to the .exe
    private readonly string _filePath;

    // 2. Thread Safety
    // Since API (Write) and Worker (Read) run in parallel, we need a lock.
    private readonly object _fileLock = new object();

    public UserRulesService()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "user_rules.json");
    }

    // 3. The Load Method
    public HashSet<string> LoadRules()
    {
        lock (_fileLock)
        {
            if (!File.Exists(_filePath))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var rulesJSON = File.ReadAllText(_filePath);
            var rules = JsonSerializer.Deserialize<List<string>>(rulesJSON);

            return new HashSet<string>(rules ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

        }
    }

    // 4. The Save Method
    public void SaveRules(List<string> processNames)
    {
        lock (_fileLock)
        {
            // A. Sanitize: Remove duplicates, trim whitespace.
            var cleanList = processNames
               .Where(p => !string.IsNullOrWhiteSpace(p))
               .Select(p => p.Trim())
               .Distinct()
               .ToList();

            // B. JsonSerializer.Serialize(processNames, options)
            var json = JsonSerializer.Serialize(cleanList, new JsonSerializerOptions { WriteIndented = true });

            // C. File.WriteAllText
            File.WriteAllText(_filePath, json);
        }
    }
}