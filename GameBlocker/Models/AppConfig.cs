using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GameBlocker.Models;

public class AppConfig
{
    [JsonPropertyName("BlockedProcesses")]
    public List<string> BlockedProcesses { get; set; }

}
