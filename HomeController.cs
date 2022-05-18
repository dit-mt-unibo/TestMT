using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TestMT.Utils;

namespace TestMT
{
    public class HomeController : Controller
    {
        
        private readonly ITranslator _translator;
        public HomeController(ITranslator translator)
        {
            _translator = translator;
        }
        public IActionResult Index()
        {
            var text = "Translate using ModernMT";
            var translatedText = _translator.TranslateText(ITranslator.Engines.ModernMT, text, "ta");
            return View("Index", translatedText);
        }

        public IActionResult Translate()
        {
            return View();
        }
        public IActionResult TranslateHtml()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
