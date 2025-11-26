using GameBlocker.Models;
using System.Text.Json.Serialization;

namespace GameBlocker.Infrastructure;

[JsonSerializable(typeof(AppConfig))]
internal partial class AppConfigJsonContext : JsonSerializerContext
{
}