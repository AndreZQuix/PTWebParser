using OpenQA.Selenium;
using System.Collections.Generic;
using System.IO;

namespace PTWebParser
{
    interface IWebParser
    {
        public static string DocFolderPath = string.Empty;
        public static int AmountOfFiles; // amount of files to parse at one iteration
        public static int Counter = 1;

        public void InitializeProperties();
        public bool IsFileCorrect();
        public List<IProduct> StartParsing(string File);
        public void GetObjectPropertiesFromTXT(ref StreamReader sr, ref IProduct pr);
        public void GetObjectPropertiesFromCSV(ref IProduct pr, ref string line);
        public void UpdateConfig(bool isEndOfFile);
        public void TryToParse(ref IWebDriver driver, ref IProduct pr);

    }
}
