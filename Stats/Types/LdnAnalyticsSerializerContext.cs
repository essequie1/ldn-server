using System.Text.Json.Serialization;

namespace LanPlayServer.Stats.Types
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(LdnAnalytics))]
    internal partial class LdnAnalyticsSerializerContext : JsonSerializerContext
    {
    }
}