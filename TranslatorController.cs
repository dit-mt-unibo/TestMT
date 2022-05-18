using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMT.Utils;


namespace TestMT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslatorController : ControllerBase
    {
        private readonly ITranslator _translator;
        public TranslatorController(ITranslator translator)
        {
            _translator = translator;
        }

        // use only space as separator, because diffs in punctuation are actually relevant
        private static char[] seps = { ' ', '\n', };
        private static string pattern = "([.,; ])";

        private static string redTagB = "<span style=\"color:red\">";
        private static string redTagE = "</span>";

        private static string gTagB = "<span style=\"color:blue\">";
        private static string gTagE = "</span>";

        private static string blackTagB = "<span style=\"color:black\">";
        private static string blackTagE = "</span>";


        private static string tagS ="<span";

        private static string template = "<table><tr><td width=\"25%\"><b>Google&nbsp;&nbsp;</b><p/></td><td>{0}<p/></td></tr><tr><td width=\"25%\"><b>ModernMT&nbsp;</b><p/></td><td>{1}<p/></td></tr><tr><td width=\"25%\"><b>{3}&nbsp;&nbsp;</b><p/></td><td>{2}<p/></td></tr></table>";
        private static string templateShort = "<table><tr><td width=\"25%\"><b>Google&nbsp;&nbsp;</b><p/></td><td>{0}<p/></td></tr><tr><td width=\"25%\"><b>Microsoft&nbsp;</b><p/></td><td>{1}<p/></td></tr></table>";

        private static int LookAheadLimit = 25; // how many words we look ahead, searching for a match
        internal class BagOfWords
        {
            public string[] words;
            public int[] points;
            public int Length { get { return words.Length; } }

            public BagOfWords(string original)
            {
                words = SplitSpecial(original.Trim());
                points = new int[words.Length];
            }

            public string PrettyPrint()
            {
                for(int i=0; i<words.Length; i++)
                {
                    if ((words[i].Length == 1) && char.IsPunctuation(words[i][0]))
                        continue;

                    if (points[i] == 2) // black
                    {
                        words[i] = blackTagB + words[i] + blackTagE;
                    }
                    else if(points[i] == 1) // blue
                    {
                        words[i] = gTagB + words[i] + gTagE;
                    }
                    else if(points[i] == 0) // red
                    {
                        words[i] = redTagB + words[i] + redTagE;
                    }
                }                
                string resp = JoinSpecial(this.words);
                return resp;
            }
        }

        /// <summary>
        /// Compute differences between first translation (taken as ref) and the others, 
        /// coloring words that are different.
        /// </summary>
        /// <param name="google"></param>
        /// <param name="mmt"></param>
        /// <param name="third"></param>
        /// <param name="thirdEngine">The name of the third engine/human used to translate an input text</param>
        /// <returns>Formatted html table using the const template defined above.</returns>
        private static string PrintResponse(string google, string mmt, string third, string thirdEngine)
        {


            var gBag = new BagOfWords(google);
            var mBag = new BagOfWords(mmt); //mmt.Split(seps);
            var dBag = new BagOfWords(third);  // third.Split(seps);

            int cm1 = 0, cd1 = 0;

            for(int cg=0; cg<gBag.Length; cg++)
            {

                for(int cm=cm1; cm<mBag.Length; cm++)
                {
                    if(gBag.words[cg] == mBag.words[cm])
                    {
                        gBag.points[cg]++;
                        mBag.points[cm]++;
                        cm1 = cm;
                        break;
                    }
                }

                for (int cd = cd1; cd < dBag.Length; cd++)
                {
                    if (gBag.words[cg] == dBag.words[cd])
                    {
                        gBag.points[cg]++;
                        dBag.points[cd]++;
                        cd1 = cd;
                        break;
                    }
                }
            }

            cd1 = 0; // reset cd1, we compare mBag and dBag from beginning
            for (int cm = 0; cm < mBag.Length; cm++)
            {
                for (int cd = cd1; cd < dBag.Length; cd++)
                {
                    if (mBag.words[cm] == dBag.words[cd])
                    {
                        mBag.points[cm]++;
                        dBag.points[cd]++;
                        cd1 = cd;
                        break;
                    }
                }
            }

            // put together strings
            string gResp = gBag.PrettyPrint();
            string mResp = mBag.PrettyPrint();
            string dResp = dBag.PrettyPrint();
            return string.Format(template, gResp, mResp, dResp, thirdEngine);
        }

        public static string[] SplitSpecial(string input)
        {
            string[] firstPass = Regex.Split(input, pattern);
            List<string> second = new List<string>();
            foreach(string s in firstPass)
            {
                if (!string.IsNullOrWhiteSpace(s))
                    second.Add(s);
            }
            return second.ToArray();
        }

        public static string JoinSpecial(string[] words)
        {
            string result = string.Empty;
            foreach(string s in words)
            {
                if((s.Length == 1) && char.IsPunctuation(s[0]))
                {
                    result += s;
                }
                else
                {
                    result += " " + s;
                }
            }
            return result;
        }

        /// <summary>
        /// Compute differences between first translation (taken as ref) and the others, 
        /// coloring words that are different.
        /// </summary>
        /// <param name="reference">The reference translation considered good</param>
        /// <param name="alternative">An alternative translation for which we highlight differences</param>
        /// <returns>Formatted html text</returns>
        private static string DiffStrings(string reference, string alternative)
        {
            // trim strings first:
            reference = reference.Trim();
            alternative = alternative.Trim();

            // prova a colorare le parole DIVERSE
            // per ogni parola google, se NON compare in alternative colora diverso, altrimenti tagga sulla lista alternative
            var refBag = SplitSpecial(reference);  //reference.Split(seps);
            var altBag = SplitSpecial(alternative);  //alternative.Split(seps);

            int cRef = 0, cAlt = 0; 
            bool matchingAlt = false;

            while (cRef < refBag.Length)
            {
                bool diffRef = false;
                matchingAlt = false;
                if (refBag[cRef] == altBag[cAlt])
                {
                    // tag as black (normal, equal) the word in Alt
                    if(altBag[cAlt].Length > 1)
                        altBag[cAlt] = blackTagB + altBag[cAlt] + blackTagE;
                    matchingAlt = true;
                    cAlt++;
                }
                else
                {
                    // does the current word in Ref appear AT ALL in Alt?
                    // look ahead..
                    int cAltForward = cAlt + 1;
                    while (cAltForward < altBag.Length)
                    {
                        if (refBag[cRef] == altBag[cAltForward])
                        {
                            if (altBag[cAltForward].Length > 1)
                            {
                                if (Math.Abs(cAltForward - cRef) < LookAheadLimit)
                                {
                                    altBag[cAltForward] = blackTagB + altBag[cAltForward] + blackTagE;
                                    if (refBag[cRef] == ".") // if this was end of sentence, update cAlt
                                        cAlt = cAltForward;
                                }
                                //else // too far, color it red
                                  //  altBag[cAltForward] = redTagB + altBag[cAltForward] + redTagE;
                            }
                            break;
                        }
                        else
                            cAltForward++;
                    }

                    if (cAltForward == altBag.Length) // not found
                    {
                        // gBag[cg] = redTagB + gBag[cg] + redTagE;
                        diffRef = true;
                    }
                }

                cRef++;
            }

            // per ogni parola in Alt Bag: se non taggata, colora diverso
            for (cAlt = 0; cAlt < altBag.Length; cAlt++)
                if ((!altBag[cAlt].StartsWith(tagS)) && (altBag[cAlt].Length > 1))
                {
                    altBag[cAlt] = redTagB + altBag[cAlt] + redTagE;
                }

            // put together strings
            string altResp = JoinSpecial(altBag);
            return altResp;
        }

        public class TranslationRequest
        {
            public string text { get; set; }
            public string language { get; set; }
            public string sourcelanguage { get; set; }
            public string reference { get; set; }
            public string other { get; set; }

        }

        //[HttpGet]
        [HttpPost]
        [Route("Translate")]
        public ActionResult Translate([FromForm] TranslationRequest tr)
        {
            //TODO: 
            // distinguere quando usare srcLang e quando lasciare autodetect
            
            if (tr.sourcelanguage == tr.language)
                return Ok("same language for source and target, hence no translation necessary :)");

            bool mockMode = false;

            string gooTranslation, mmtTranslation, msTranslation = string.Empty;

            var thirdEngine = "Microsoft";
            if (tr.other != null) // human alternative provided. Use that and do not try to translate with MS MT
            {
                msTranslation = tr.other;
                thirdEngine = "Human (alt)";
            }

            if (mockMode)
            {
                gooTranslation = "exe uay zee enne exe";
                mmtTranslation = "exe uay doubliu enne foo exe";
                //msTranslation = "exe bee doubliu elle exe";
                msTranslation = "Anche se l’Australia è un Paese di migranti, l’attuale accoglienza dei rifugiati e dei richiedenti asilo è la causa di un risentimento sia nazionale che internazionale. In questa situazione, il caso di Behrooz Boochani, uno scrittore, regista e giornalista iraniano, nonché rifugiato, è forse il più rappresentativo. Boochani ha vissuto sei anni nel campo di rifugiati di Manus, situato nello Stato di Papua Nuova Guinea. Nel 2017, con la chiusura del campo, Boochani è stato trasferito a Port Moresby e il suo status futuro è diventato incerto.";
            }
            else
            {
                gooTranslation = _translator.TranslateText(ITranslator.Engines.Google, tr.text, tr.language, tr.sourcelanguage);
                mmtTranslation = _translator.TranslateText(ITranslator.Engines.ModernMT, tr.text, tr.language, tr.sourcelanguage);
                // var deepLTranslation = _translator.TranslateText(ITranslator.Engines.DeepL, text, language, sourcelanguage);

                if(tr.other ==null)
                {
                    msTranslation = _translator.TranslateText(ITranslator.Engines.Microsoft, tr.text, tr.language, tr.sourcelanguage);
                }
            }

            var result = string.Empty;
            if (tr.reference != null)
            {
                string coloredGoogle = DiffStrings(tr.reference, gooTranslation);
                string coloredMMT = DiffStrings(tr.reference, mmtTranslation);
                string coloredMS = DiffStrings(tr.reference, msTranslation);
                result = string.Format(template, coloredGoogle, coloredMMT, coloredMS, thirdEngine);                                
            }
            else
            {
                result = PrintResponse(gooTranslation, mmtTranslation, msTranslation, thirdEngine);
            }
            return Ok(result);
        }


        [HttpGet]
        [Route("TranslateHtml")]
        public ActionResult TranslateHtml(string html, string language)
        {
            var translatedText = _translator.TranslateHtml(html, language);
            return Ok(translatedText);
        }
    }
}