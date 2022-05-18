using System;
using SampleClient.Model;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ModernMTApi mmt = new ModernMTApi(
                "34D798B7-980B-BD3E-9DA8-EB33745019BD",
                "0.1", 
                "UniBo App", 
                "1.0"
                );

            Translation res = mmt.Translate("en", "it", "Hello World!");
            
            Console.WriteLine(res.Text);
        }
    }
}