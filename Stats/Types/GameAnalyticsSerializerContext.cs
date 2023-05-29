using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LanPlayServer.Stats.Types
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(List<GameAnalytics>))]
    internal partial class GameAnalyticsSerializerContext : JsonSerializerContext
    {
    }
}