using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TestMT.ModernMT.Model
{
    class LanguageMapConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(LanguageMap) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            LanguageMap languageMap = new LanguageMap();

            JObject json = JObject.Load(reader);
            foreach (var kv in json)
            {
                string sourceLanguage = kv.Key;
                foreach (string targetLanguage in kv.Value)
                    languageMap.Add(sourceLanguage, targetLanguage);
            }

            return languageMap;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Dictionary<string, HashSet<string>> languages = ((LanguageMap)value).ExportLanguageDictionary();

            writer.WriteStartObject();
            foreach (var entry in languages)
            {
                writer.WritePropertyName(entry.Key);
                writer.WriteStartArray();
                foreach (var language in entry.Value)
                    writer.WriteValue(language);
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}
