using OpenQA.Selenium;
using System.Collections.Generic;
using System.IO;

namespace PTWebParser
{
    interface IWebParser
    {
        public static string DocFolderPath = "../../../../Docs/";

        public void InitializeProperties();
        public bool IsFileCorrect();
        public List<IProduct> StartParsing();
        public void GetObjectProperties(ref StreamReader sr, ref IProduct pr);
        public void UpdateCounter();
        public void TryToParse(ref IWebDriver driver, ref IProduct pr);

    }
}
