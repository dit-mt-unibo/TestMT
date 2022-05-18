using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TestMT.ModernMT.Model
{
    [JsonConverter(typeof(LanguageMapConverter))]
    public class LanguageMap
    {
        public static LanguageMap FromString(string str)
        {
            LanguageMap languageMap = new LanguageMap();

            foreach (string e in str.Split(','))
            {
                string[] st = e.Split(':');
                languageMap.DoAdd(st[0], st[1]);
            }

            return languageMap;
        }

        private readonly HashSet<Tuple<string, string>> languages = new HashSet<Tuple<string, string>>();

        public void Add(CultureInfo sourceLanguage, CultureInfo targetLanguage)
        {
            DoAdd(sourceLanguage.TwoLetterISOLanguageName, targetLanguage.TwoLetterISOLanguageName);
        }

        public void Add(string sourceLanguage, string targetLanguage)
        {
            int index;

            index = sourceLanguage.IndexOf('-');
            if (index != -1)
                sourceLanguage = sourceLanguage.Substring(0, index);

            index = targetLanguage.IndexOf('-');
            if (index != -1)
                targetLanguage = targetLanguage.Substring(0, index);

            DoAdd(sourceLanguage, targetLanguage);
        }

        private void DoAdd(string sourceLanguage, string targetLanguage)
        {
            languages.Add(new Tuple<string, string>(sourceLanguage, targetLanguage));
        }

        public bool Contains(CultureInfo sourceCulture, CultureInfo targetCulture)
        {
            string sourceLanguage = sourceCulture.TwoLetterISOLanguageName;
            string targetLanguage = targetCulture.TwoLetterISOLanguageName;

            return languages.Contains(new Tuple<string, string>(sourceLanguage, targetLanguage));
        }

        public Dictionary<string, HashSet<string>> ExportLanguageDictionary()
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            foreach (var tuple in languages)
            {
                if (!result.ContainsKey(tuple.Item1))
                    result.Add(tuple.Item1, new HashSet<string>());
                result[tuple.Item1].Add(tuple.Item2);
            }
            return result;
        }
    }
}
