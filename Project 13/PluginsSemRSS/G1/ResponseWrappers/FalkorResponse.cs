using Newtonsoft.Json;
using System.Collections.Generic;

namespace TVPlayerAPI.G1.ResponseWrappers
{
    internal class FalkorResponse
    {
        [JsonProperty("items")]
        public List<FalkorPost> items { get; set; }

        [JsonIgnore]
        public bool TemConteudo
        {
            get { return items != null && items.Count > 0; }
        }
    }

    internal class FalkorPost
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("publication")]
        public string publication { get; set; }

        [JsonProperty("content")]
        public FalkorConteudo content { get; set; }
    }

    internal class FalkorConteudo
    {
        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("summary")]
        public string summary { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("image")]
        public FalkorImagem image { get; set; }
    }

    internal class FalkorImagem
    {
        [JsonProperty("sizes")]
        public Dictionary<string, FalkorTamanho> sizes { get; set; }
    }

    internal class FalkorTamanho
    {
        [JsonProperty("url")]
        public string url { get; set; }

        [JsonProperty("width")]
        public int width { get; set; }

        [JsonProperty("height")]
        public int height { get; set; }
    }
}
