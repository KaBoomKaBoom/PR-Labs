using System.Text.Json;
namespace Lab2.Helpers
{
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
}
