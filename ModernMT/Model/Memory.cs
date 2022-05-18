using Newtonsoft.Json;
using System;

namespace TestMT.ModernMT.Model
{
    public class Memory
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("externalId")]
        public string ExternalId { get; set; }
        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }
        
    }
}
