using GameBlocker.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GameBlocker.Services;

public class ConfigLoader
{
    public AppConfig LoadConfig(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize(jsonString, AppConfigJsonContext.Default.AppConfig);
    }
}
