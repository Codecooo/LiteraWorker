using System.Text.Json;

namespace LiteraWorker.Core.Helpers;

class JsonOptionsDefault
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        TypeInfoResolver = JsonContext.Default
    };
}