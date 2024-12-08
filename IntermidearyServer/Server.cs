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
        var response = await _client.PostAsync($"http://host.docker.internal:{GlobalState.Host}/products", content);

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

    public async Task SendMultipartPostRequest(string filePath)
    {
        try
        {
            using var multipartContent = new MultipartFormDataContent();

            // Open the file stream and add it to the multipart content
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileStreamContent = new StreamContent(fileStream);

            // Set the content type to "application/json" (if the file is JSON)
            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            // Add the file to the multipart form data with name "file"
            multipartContent.Add(fileStreamContent, "file", Path.GetFileName(filePath));

            // Send the POST request
            var response = await _client.PostAsync($"http://host.docker.internal:{GlobalState.Host}/upload", multipartContent);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("File successfully uploaded to the server.");
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to upload the file. Status Code: {response.StatusCode}, Details: {errorDetails}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

}