using TestMT.ModernMT.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace TestMT.ModernMT
{
    public class ModernMTApi
    {
        private readonly string pluginVersion;
        private readonly string platform;
        private readonly string platformVersion;

        private readonly HttpClient httpClient;

        public string LicenseKey;
        public int Client;

        public ModernMTApi(string licenseKey, string pluginVersion, string platform, string platformVersion)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            this.pluginVersion = pluginVersion;
            this.platform = platform;
            this.platformVersion = platformVersion;
            LicenseKey = licenseKey;
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.modernmt.com/"),
                Timeout = new TimeSpan(1, 0, 0)
            };
        }

        #region Miscellaneous APIs

        public LanguageMap GetLanguages()
        {
            return Send<LanguageMap>("GET", "languages");
        }

        #endregion

        #region Memory APIs

        public List<Memory> GetAllMemories()
        {
            return Send<List<Memory>>("GET", "memories");
        }

        public Memory GetMemoryById(long id)
        {
            return Send<Memory>("GET", "memories/" + id.ToString());
        }

        public Memory CreateMemory(string name, string description = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "name", name },
                { "description", description }
            };

            return Send<Memory>("POST", "memories", parameters);
        }

        public Memory DeleteMemory(long id)
        {
            return Send<Memory>("DELETE", "memories/" + id.ToString());
        }

        public Memory UpdateMemory(long id, string name, string description = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "name", name },
                { "description", description}
            };

            return Send<Memory>("PUT", "memories/" + id.ToString(), parameters);
        }

        public ImportJob AddOrReplaceToMemoryContent(long id, string sourceLanguage, string targetLanguage, string sentence, string translation, string tuid)
        {
            string endpoint = "memories/" + id.ToString() + "/content";
            string method = "PUT";

            var parameters = new Dictionary<string, string>
            {
                { "source", sourceLanguage }, { "target", targetLanguage },
                { "sentence", sentence }, { "translation", translation },
                { "tuid", tuid }
            };

            return Send<ImportJob>(method, endpoint, parameters);
        }

        public ImportJob ImportIntoMemoryContent(long id, string tmxPath, bool compression = false)
        {
            var parameters = new Dictionary<string, string> { { "compression", compression ? "gzip" : null } };
            var multipart = new Dictionary<string, string> { { "tmx", tmxPath } };

            return Send<ImportJob>("POST", "memories/" + id.ToString() + "/content", parameters, multipart);
        }

        public ImportJob GetImportJob(string uuid)
        {
            return Send<ImportJob>("GET", "import-jobs/" + uuid);
        }

        #endregion

        #region Translation APIs

        public ContextVector GetContextVectorFromString(string sourceLanguage, string targetLanguage, string text, long[] hints = null, int limit = 0)
        {
            var parameters = new Dictionary<string, string>
            {
                { "source", sourceLanguage }, { "targets", targetLanguage },
                { "text", text },
                { "limit", limit > 0 ? limit.ToString() : null },
                { "hints", hints != null && hints.Length > 0 ? string.Join(",", hints.Select(x => x.ToString())) : null }
            };

            return Send<ContextVector>("GET", "context-vector", parameters);
        }

        public ContextVector GetContextVectorFromFile(string sourceLanguage, string targetLanguage, string filePath, bool compression = false, long[] hints = null, int limit = 0)
        {
            var parameters = new Dictionary<string, string>
            {
                { "source", sourceLanguage }, { "targets", targetLanguage },
                { "compression", compression ? "gzip" : null },
                { "limit", limit > 0 ? limit.ToString() : null },
                { "hints", hints != null && hints.Length > 0 ? string.Join(",", hints.Select(x => x.ToString())) : null }
            };

            var multipart = new Dictionary<string, string> { { "content", filePath } };

            return Send<ContextVector>("GET", "context-vector", parameters, multipart);
        }

        public Translation Translate(string sourceLanguage, string targetLanguage, string text, string contextVector = null, long[] hints = null, string projectId = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "source", sourceLanguage }, { "target", targetLanguage }, { "q", text }, { "context_vector", contextVector },
                { "project_id", projectId },
                { "hints", hints != null && hints.Length > 0 ? string.Join(",", hints.Select(x => x.ToString()).ToArray()) : null }
            };

            return Send<Translation>("GET", "translate", parameters);
        }

        #endregion

        #region Low level methods

        private dynamic Send<T>(string method, string endpoint, Dictionary<string, string> parameters = null, Dictionary<string, string> multipart = null)
        {
            return DoSend<T>(method, endpoint, parameters, multipart);
        }

        private dynamic DoSend<T>(string method, string endpoint, Dictionary<string, string> parameters = null, Dictionary<string, string> multipart = null)
        {
            if (parameters != null)
                parameters = parameters.Where(entry => entry.Value != null).ToDictionary(entry => entry.Key, entry => entry.Value);
            if (multipart != null)
                multipart = multipart.Where(entry => entry.Value != null).ToDictionary(entry => entry.Key, entry => entry.Value);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (!"POST".Equals(method))
                request.Headers.Add("X-HTTP-Method-Override", method);

            if (LicenseKey != null)
                request.Headers.Add("MMT-ApiKey", LicenseKey);
            if (Client > 0)
                request.Headers.Add("MMT-ApiClient", Client.ToString());
            if (pluginVersion != null)
                request.Headers.Add("MMT-PluginVersion", pluginVersion);
            if (platform != null)
                request.Headers.Add("MMT-Platform", platform);
            if (platformVersion != null)
                request.Headers.Add("MMT-PlatformVersion", platformVersion);

            if (multipart != null && multipart.Count > 0)
            {
                MultipartFormDataContent content = new MultipartFormDataContent("---------------------------------------BUsrRFW5TN8EzeXLnqVsxUHcuHV");
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (KeyValuePair<string, string> entry in parameters)
                    {
                        content.Add(new StringContent(entry.Value, System.Text.Encoding.UTF8), entry.Key);
                    }
                }

                foreach (KeyValuePair<string, string> entry in multipart)
                {
                    FileStream stream = File.Open(entry.Value, FileMode.Open);
                    content.Add(new StreamContent(stream), entry.Key, Path.GetFileName(entry.Value));
                }

                request.Content = content;
            }
            else if (parameters != null && parameters.Count > 0)
            {
                request.Content = new FormUrlEncodedContent(parameters);
            }

            string rawContent;

            try
            {
                var requestTask = httpClient.SendAsync(request);
                requestTask.Wait();
                var contentTask = requestTask.Result.Content.ReadAsStringAsync();
                contentTask.Wait();
                rawContent = contentTask.Result;
            }
            catch (AggregateException e)
            {
                Exception inner = e.InnerException;
                if (inner is HttpRequestException)
                    throw inner;
                else
                    throw ApiException.UnexpectedException(inner);
            }

            return Parse(typeof(T), rawContent);
        }

        private dynamic Parse(Type resultType, string content)
        {
            try
            {
                JObject root = JObject.Parse(content);
                int status = root["status"].ToObject<int>();
                if (status < 200 || status > 299)
                    throw ApiException.FromJson(root);

                JToken data = root["data"];
                return data.ToObject(resultType);
            }
            catch (JsonException e)
            {
                throw new ApiException(500, "InvalidJsonException", "Unable to decode server response", e);
            }
        }

        #endregion
    }
}
