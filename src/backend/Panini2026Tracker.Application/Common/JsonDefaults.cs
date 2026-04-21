using System.Text.Json;

namespace Panini2026Tracker.Application.Common;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}
