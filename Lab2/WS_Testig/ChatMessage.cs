
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

public class ChatMessage
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Room { get; set; }
    public string? Content { get; set; }

    public static string Serialize(ChatMessage message)
    {
        return JsonSerializer.Serialize(message);
    }

    public static ChatMessage Deserialize(string json)
    {
        return JsonSerializer.Deserialize<ChatMessage>(json);
    }
}
