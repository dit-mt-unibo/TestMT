using Newtonsoft.Json;

namespace TestMT.ModernMT.Model
{
    public class Translation
    {
        [JsonProperty("translation")]
        public string Text { get; set; }
    }
}
