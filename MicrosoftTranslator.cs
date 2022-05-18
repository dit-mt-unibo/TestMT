using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
// Install Newtonsoft.Json with NuGet
using Newtonsoft.Json;

namespace TestMT
{
    public class MicrosoftTranslatorClient
    {
        /// <summary>
        /// The C# classes that represents the JSON returned by the Translator Text API.
        /// </summary>
        public class TranslationResult
        {
            public DetectedLanguage DetectedLanguage { get; set; }
            public TextResult SourceText { get; set; }
            public Translation[] Translations { get; set; }
        }

        public class DetectedLanguage
        {
            public string Language { get; set; }
            public float Score { get; set; }
        }

        public class TextResult
        {
            public string Text { get; set; }
            public string Script { get; set; }
        }

        public class Translation
        {
            public string Text { get; set; }
            public TextResult Transliteration { get; set; }
            public string To { get; set; }
            public Alignment Alignment { get; set; }
            public SentenceLength SentLen { get; set; }
        }

        public class Alignment
        {
            public string Proj { get; set; }
        }

        public class SentenceLength
        {
            public int[] SrcSentLen { get; set; }
            public int[] TransSentLen { get; set; }
        }

        private readonly string subscriptionKey; // = "YOUR-SUBSCRIPTION-KEY";
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";

        // Add your location, also known as region. The default is global.
        // This is required if using a Cognitive Services resource.
        private static readonly string location = "westeurope";

        public MicrosoftTranslatorClient(string subKey)
        {
            subscriptionKey = subKey;
        }

        public async Task<string> Translate(string srcLanguage, string tgtLanguage, string text)
        {
            // Output languages are defined in the route.
            // For a complete list of options, see API reference.
            // https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
            string route = "/translate?api-version=3.0&from={0}&to={1}";
            string routeFilled = string.Format(route, srcLanguage, tgtLanguage);
            return await TranslateTextRequest(subscriptionKey, endpoint, routeFilled, text);
        }

        // Async call to the Translator Text API
        static public async Task<string> TranslateTextRequest(string subscriptionKey, string endpoint, string route, string inputText)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            string translationText = null;

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                if(!response.IsSuccessStatusCode)
                {
                    // handle error..
                    return "ERROR " + response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync();
                }
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
                // Iterate over the deserialized results.
                foreach (TranslationResult o in deserializedOutput)
                {
                    foreach (Translation t in o.Translations)
                    {
                        return t.Text;
                    }
                }
            }

            return translationText;
        }
    }
}

