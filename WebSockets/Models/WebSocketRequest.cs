using System.Text.Json;

public record WebSocketRequest(string RequestId, string Method, object Data)
{
    public T GetData<T>()
    {
        if (Data is T directCast)
        {
            return directCast;
        }

        if (Data is JsonElement jsonElement)
        {
            return jsonElement.Deserialize<T>() ?? throw new FormatException("The given data given as parameter is not compatible with the method");
        }

        var json = JsonSerializer.Serialize(Data);
        return JsonSerializer.Deserialize<T>(json) ?? throw new FormatException("The given data given as parameter is not compatible with the method");
    }
}