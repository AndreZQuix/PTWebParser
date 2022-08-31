using OpenQA.Selenium;
using System.Collections.Generic;
using System.IO;

namespace PTWebParser
{
    interface IWebParser
    {
        public static string DocFolderPath = string.Empty;
        public static int AmountOfFiles = 10; // amount of files to parse at one iteration
        public static int Counter = 1;

        public bool InitializeProperties();
        public bool IsFileCorrect();
        public List<IProduct> StartParsing(string File);
        public bool GetObjectPropertiesFromCSV(ref IProduct pr, ref string line);
        public void UpdateConfig(bool isEndOfFile);
        public void TryToParse(ref IWebDriver driver, ref IProduct pr);
    }
}
