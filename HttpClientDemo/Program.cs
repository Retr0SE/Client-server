using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

public class GhibliFilm
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string director { get; set; } = string.Empty;
    public string release_date { get; set; } = string.Empty;
}

public class PostData
{
    public string title { get; set; } = string.Empty;
    public string body { get; set; } = string.Empty;
    public int userId { get; set; }
}

class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string getUrl = "https://ghibliapi.vercel.app/films";
        string postUrl = "https://jsonplaceholder.typicode.com/posts";
        List<GhibliFilm> films = new List<GhibliFilm>();

        Console.WriteLine("===1. Отримуємо список фільмів (GET)===");
        try
        {
            HttpResponseMessage getResponse = await client.GetAsync(getUrl);
            getResponse.EnsureSuccessStatusCode();

            string jsonResponse = await getResponse.Content.ReadAsStringAsync();
            films = JsonSerializer.Deserialize<List<GhibliFilm>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<GhibliFilm>();

            Console.WriteLine($"Успішно отримано {films.Count} фільмів:");
            foreach (var film in films)
            {
                Console.WriteLine($"- {film.title} ({film.release_date}), режисер: {film.director}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Помилка GET-запиту: {e.Message}");
        }

        Console.WriteLine("\n===2. Створюємо тестовий пост (POST)===");
        var newPost = new PostData
        {
            title = "API Test Post",
            body = "Hello from HttpClient",
            userId = 1
        };

        string jsonPayload = JsonSerializer.Serialize(newPost);
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage postResponse = await client.PostAsync(postUrl, content);
            postResponse.EnsureSuccessStatusCode();

            string postResult = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Успішно створено пост! Статус: {(int)postResponse.StatusCode}");
            Console.WriteLine($"Відповідь сервера: {postResult}");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Помилка POST-запиту: {e.Message}");
        }

        string filePath = "ghibli_films.csv";
        Console.WriteLine($"\n===3. Зберігаємо дані у файл {filePath} ===");

        if (films.Count == 0)
        {
            Console.WriteLine("Помилка: Не вдалося отримати дані, CSV файл не буде створено.");
        }
        else
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("ID,Title,Director,ReleaseDate");
                    foreach (var film in films)
                    {
                        writer.WriteLine($"{film.id},{film.title},{film.director},{film.release_date}");
                    }
                }
                Console.WriteLine($"Дані про {films.Count} фільмів успішно збережено у файл: {Path.GetFullPath(filePath)}");
            }
            catch (IOException e)
            {
                Console.WriteLine($"Помилка при записі у файл: {e.Message}");
            }
        }
    }
}
