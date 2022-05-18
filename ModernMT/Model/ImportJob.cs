using Newtonsoft.Json;
using System;

namespace TestMT.ModernMT.Model
{
    public class ImportJob
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("begin")]
        public long Begin { get; set; }
        [JsonProperty("end")]
        public long End { get; set; }
        [JsonProperty("dataChannel")]
        public int DataChannel { get; set; }
        [JsonProperty("progress")]
        public float Progress { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }

        public int PercentProgress { get { return (int)Math.Round(Progress * 100); } }
    }
}
