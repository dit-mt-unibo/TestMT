using Newtonsoft.Json;
using System.Collections.Generic;

namespace TestMT.ModernMT.Model
{
    public class ContextVector
    {
        [JsonProperty("source")]
        public string SourceLanguage;
        [JsonProperty("vectors")]
        public Dictionary<string, string> Vectors;

        public string GetVectorString(string target)
        {
            if (!Vectors.ContainsKey(target))
                return null;

            return Vectors[target].Length > 0 ? Vectors[target] : null;
        }
    }
}
