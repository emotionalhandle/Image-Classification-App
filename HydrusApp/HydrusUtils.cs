using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

public class HydrusUtils
{
    private readonly HttpClient _client;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public HydrusUtils(string apiKey, string baseUrl = "http://localhost:45869")
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", apiKey);
    }

    public async Task<Dictionary<string, HashSet<string>>> GetSourceTags(string fileHash)
    {
        // Get file metadata which includes tags
        var response = await _client.GetAsync($"{_baseUrl}/get_files/file_metadata?hash={fileHash}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var metadata = doc.RootElement.GetProperty("metadata")[0];
        
        var result = new Dictionary<string, HashSet<string>>();

        // Extract tags from each service
        if (metadata.TryGetProperty("tags", out var tagsElement))
        {
            foreach (var serviceProperty in tagsElement.EnumerateObject())
            {
                var serviceKey = serviceProperty.Name;
                var serviceTags = new HashSet<string>();

                // Get storage tags (raw tags before processing)
                if (serviceProperty.Value.TryGetProperty("storage_tags", out var storageTags))
                {
                    foreach (var statusProperty in storageTags.EnumerateObject())
                    {
                        var tagArray = statusProperty.Value.EnumerateArray();
                        foreach (var tag in tagArray)
                        {
                            var tagString = tag.GetString();
                            if (tagString != null)
                            {
                                serviceTags.Add(tagString);
                            }
                        }
                    }
                }

                if (serviceTags.Count > 0)
                {
                    result[serviceKey] = serviceTags;
                }
            }
        }

        return result;
    }


    public async Task<List<Page>> GetPages()
    {
        try
        {
            var response = await _client.GetAsync($"{_baseUrl}/manage_pages/get_pages");
            response.EnsureSuccessStatusCode();
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Got response from API");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = false // We're using explicit property names now
            };
            
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;
            var pagesObject = root.GetProperty("pages");
            
            Console.WriteLine("Parsed JSON document");
            
            // First, deserialize the top-level page
            var topPage = JsonSerializer.Deserialize<Page>(pagesObject.GetRawText(), options);
            
            if (topPage == null)
            {
                throw new InvalidOperationException("Failed to deserialize top-level page");
            }
            
            Console.WriteLine($"Deserialized top page: {topPage.Name}");
            
            // Flatten the hierarchical structure to get all pages
            var allPages = new List<Page>();
            FlattenPages(topPage, allPages);
            
            Console.WriteLine($"Flattened {allPages.Count} pages");
            return allPages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPages: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void FlattenPages(Page page, List<Page> allPages)
    {
        allPages.Add(page);
        if (page.Pages != null)
        {
            foreach (var subPage in page.Pages)
            {
                FlattenPages(subPage, allPages);
            }
        }
    }

    public async Task AddFileToPage(string pageKey, string fileHash)
    {
        var content = new
        {
            page_key = pageKey,
            hash = fileHash
        };
        
        var json = JsonSerializer.Serialize(content);
        var stringContent = new StringContent(
            json, 
            Encoding.UTF8, 
            "application/json"
        );
        
        var response = await _client.PostAsync("http://localhost:45869/manage_pages/add_files", stringContent);
        response.EnsureSuccessStatusCode();
    }
}

public class Page
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("page_key")]
    public string? PageKey { get; set; }
    
    [JsonPropertyName("page_state")]
    public int PageState { get; set; }
    
    [JsonPropertyName("page_type")]
    public int PageType { get; set; }
    
    [JsonPropertyName("is_media_page")]
    public bool IsMediaPage { get; set; }
    
    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
    
    [JsonPropertyName("pages")]
    public List<Page>? Pages { get; set; }
}

public class PagesResponse
{
    public Page? Pages { get; set; }
    public int Version { get; set; }
    public int HydrusVersion { get; set; }
}