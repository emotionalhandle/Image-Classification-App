using System.Text.Json.Serialization;

namespace HydrusApp.Models
{
    public class Page
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("page_key")]
        public string PageKey { get; set; } = string.Empty;

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
        [JsonPropertyName("pages")]
        public Page? RootPage { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("hydrus_version")]
        public int HydrusVersion { get; set; }
    }
} 