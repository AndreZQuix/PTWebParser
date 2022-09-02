using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace PTWebParser
{
    interface IWebParser
    {
        public static string DocFolderPath = string.Empty;
        public static int AmountOfFiles = 10; // amount of files to parse at one iteration
        public static int Counter = 1;

        public bool InitializeConfig();
        public bool InitializeSettings(string file);
        public bool IsFileCorrect();
        public List<IProduct> StartParsing(string file, string settings);
        public bool GetObjectPropertiesFromCSV(ref IProduct pr, ref string line);
        public void UpdateConfig(bool isEndOfFile);
        public void TryToParse(ref IWebDriver driver, ref IProduct pr);
    }
}
