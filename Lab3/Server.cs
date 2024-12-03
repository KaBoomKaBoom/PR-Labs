using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


public class Server
{
    private HttpClient _client = new HttpClient();

    public async Task SendPostRequest(string json)
    {
        var content = new StringContent("[" + json + "]", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("http://localhost:8080/products", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {response.StatusCode}, Details: {errorDetails}");
        }
        else
        {
            Console.WriteLine($"Sent message to database: {json}");
        }
    }

}