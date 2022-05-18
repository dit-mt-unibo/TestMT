using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Cloud.Translation.V2;
using TestMT.ModernMT;
using TestMT.ModernMT.Model;
using Microsoft.Extensions.Configuration;

namespace TestMT.Utils
{
    public interface ITranslator
    {
        string TranslateText(Engines engine, string text, string tgtLanguage, string srcLanguage = null);
        string TranslateHtml(string text, string language);

        enum Engines { Google = 0, ModernMT = 1, DeepL = 2, Microsoft = 3 };
    }

    public class Translator : ITranslator
    {
        private readonly TranslationClient client;
        private readonly ModernMTApi mmtClient;
        private readonly DeepL.DeepLClient deepLClient;
        private readonly MicrosoftTranslatorClient msClient;

        // requires using Microsoft.Extensions.Configuration;
        private readonly IConfiguration Configuration;

        public Translator(IConfiguration configuration)
        {
            Configuration = configuration;

            client = TranslationClient.Create(
                Google.Apis.Auth.OAuth2.GoogleCredential.FromStream(
                    new FileStream("./unibo-literary-translation-09aff375131b.json", FileMode.Open)));

            string mmtKey = Configuration["TranslatorKeys:ModernMT"];
            string msKey = Configuration["TranslatorKeys:Microsoft"];

            mmtClient = new ModernMTApi(
                mmtKey,
                "0.1", 
                "UniBo App", 
                "1.0"
                );

            // deepLClient = new DeepL.DeepLClient("566c288a-30a3-e35e-d9b3-8206c6355816");

            msClient = new MicrosoftTranslatorClient(msKey);
    }

    public string TranslateHtml(string text, string language)
        {
            var response = client.TranslateHtml(text, language);
            return response.TranslatedText;
        }

        public string TranslateText(ITranslator.Engines engine, string text, string tgtLanguage, string srcLanguage = null)
        {
            long[] hints = new long[] { 33860 }; // memories used by my ModernMT account.

            switch (engine)
            {
                case ITranslator.Engines.Google:
                    var response = client.TranslateText(text, tgtLanguage, srcLanguage);
                    return response.TranslatedText;
                    //return "All good men will come to a rescue of green people";
                case ITranslator.Engines.ModernMT:
                    Translation mmtRes = mmtClient.Translate(srcLanguage, tgtLanguage, text, null, hints);
                    return mmtRes.Text;
                //return "All good men will come to the rescue of bad people";
                case ITranslator.Engines.DeepL:
                    Task<DeepL.Translation> task;
                    if (srcLanguage != null) 
                    {
                        task = deepLClient.TranslateAsync( text, srcLanguage.ToUpperInvariant(), tgtLanguage.ToUpperInvariant());
                    }
                    else
                    {
                        task = deepLClient.TranslateAsync( text, tgtLanguage.ToUpperInvariant());
                    }
                    task.Wait();
                    DeepL.Translation translation = task.Result;
                    return translation.Text;
                case ITranslator.Engines.Microsoft:
                    Task<string> msTask;
                    msTask = msClient.Translate(srcLanguage, tgtLanguage, text);
                    msTask.Wait();
                    string translatedText = msTask.Result;
                    return translatedText;
                  
            }

            return string.Empty;
        }
    }
}