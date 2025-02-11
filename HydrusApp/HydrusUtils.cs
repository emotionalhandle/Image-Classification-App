using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Linq;
using HydrusApp.Models;
using System.Text.Json.Serialization;

public static class HydrusUtils
{
    private static readonly string _baseUrl = "http://127.0.0.1:45869";
    private static readonly string _apiKey = "5a880bb8e976458d386516747c4cb070be8da0464789d1415b1c87c76660648d";

    public static async Task<List<Page>> GetPages()
    {
        string content = string.Empty;
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", _apiKey);

            var response = await client.GetAsync($"{_baseUrl}/manage_pages/get_pages");
            content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Got response from API");
            Console.WriteLine($"Response status: {response.StatusCode}");
            Console.WriteLine($"Response content length: {content.Length}");
            Console.WriteLine($"First 100 characters of response: {content.Substring(0, Math.Min(100, content.Length))}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };

            var pagesResponse = JsonSerializer.Deserialize<PagesResponse>(content, options);
            if (pagesResponse?.RootPage == null)
            {
                Console.WriteLine("Warning: API returned null response");
                return new List<Page>();
            }

            var allPages = new List<Page>();
            CollectPages(pagesResponse.RootPage, allPages);
            Console.WriteLine($"Found {allPages.Count} pages");
            return allPages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing response: {ex.Message}");
            Console.WriteLine($"Full response content: {content}");
            throw;
        }
    }

    private static void CollectPages(Page page, List<Page> allPages)
    {
        allPages.Add(page);
        if (page.Pages != null)
        {
            foreach (var subPage in page.Pages)
            {
                CollectPages(subPage, allPages);
            }
        }
    }

    public static async Task<string> GetPageKeyByPath(string path)
    {
        var pages = await GetPages();
        
        // First, check if the path is already a page key
        foreach (var page in pages)
        {
            if (page.PageKey == path)
            {
                return page.PageKey;
            }
        }
        
        // If not a page key, try to find by name
        var pathParts = path.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var currentPage = pages.FirstOrDefault(p => p.Name.Equals(pathParts[0], StringComparison.OrdinalIgnoreCase));
        
        if (currentPage == null)
        {
            throw new Exception($"Could not find page with name: {pathParts[0]}");
        }

        for (int i = 1; i < pathParts.Length; i++)
        {
            var nextPage = currentPage.Pages?.FirstOrDefault(p => p.Name.Equals(pathParts[i], StringComparison.OrdinalIgnoreCase));
            if (nextPage == null)
            {
                throw new Exception($"Could not find page with name: {pathParts[i]} under {currentPage.Name}");
            }
            currentPage = nextPage;
        }

        return currentPage.PageKey;
    }

    public static async Task AddFileToPage(string pageKey, string fileHash)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", _apiKey);

        var content = new StringContent($"{{\"hash\": \"{fileHash}\", \"page_key\": \"{pageKey}\"}}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{_baseUrl}/add_files/add_file_to_page", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to add file to page: {response.StatusCode}");
        }
    }
}

public class Page
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("page_key")]
    public string PageKey { get; set; } = string.Empty;
    
    [JsonPropertyName("pages")]
    public List<Page> Pages { get; set; } = new List<Page>();
    
    [JsonPropertyName("page_state")]
    public PageState? PageState { get; set; }
}

public class PagesResponse
{
    [JsonPropertyName("pages")]
    public List<Page> Pages { get; set; } = new List<Page>();

    [JsonPropertyName("root_page")]
    public Page? RootPage { get; set; }
}

public class PageState
{
    [JsonPropertyName("is_media_page")]
    public bool IsMediaPage { get; set; }
}