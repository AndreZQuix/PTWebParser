using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PTWebParser
{
    public class WebParser : IWebParser
    {
        List<IProduct> products = new List<IProduct>();

        public string DocFolderPath = "Docs/";
        public static int AmountOfFiles = 10;
        public static int Counter = 1;

        private string ParsedLink = String.Empty;
        private string ParsedFile = String.Empty;
        private string SelectorTitle = String.Empty;
        private string SelectorName = String.Empty; // CSS selector to parse product name from web page
        private string SelectorPrice = String.Empty; // CSS selector to parse other product price from web page
        private string AttributeName = "textContent";   // attribute name to get from CSS selector
        private string TextToReplace = String.Empty;
        private bool IsNomenLoaded = false;

        private string TempURL = String.Empty;

        public bool InitializeProperties() // read config and get parsing values
        {
            try
            {
                foreach(string line in File.ReadAllLines(DocFolderPath + "config.ini"))
                {
                    if (line.Contains("ParsedLink:"))
                        ParsedLink = line.Replace("ParsedLink:", "");

                    if(line.Contains("Counter:"))
                    {
                        TextToReplace = line;
                        Counter = Convert.ToInt32(line.Replace("Counter:", ""));
                    }

                    if (line.Contains("AmountOfFiles:"))
                        AmountOfFiles = Convert.ToInt32(line.Replace("AmountOfFiles:", ""));

                    if (line.Contains("Nomenclature:"))
                    {
                        ParsedFile = line.Replace("Nomenclature:", "");
                        IsNomenLoaded = ParsedFile.Length != 0;
                    }

                    if (line.Contains("TitleSelector:"))
                        SelectorTitle = line.Replace("TitleSelector:", "");

                    if (line.Contains("NameSelector:"))
                        SelectorName = line.Replace("NameSelector:", "");

                    if (line.Contains("PriceSelector:"))
                        SelectorPrice = line.Replace("PriceSelector:", "");
                }
            }
            catch(Exception ex)
                { MessageBox.Show("Невозможно открыть config.ini: " + ex.Message);   }

            return IsNomenLoaded;
        }

        public bool IsFileCorrect()
        {
            return !string.IsNullOrEmpty(TextToReplace) && !string.IsNullOrEmpty(ParsedLink) && !string.IsNullOrEmpty(SelectorTitle) 
                && !string.IsNullOrEmpty(SelectorPrice) && !string.IsNullOrEmpty(SelectorName);
        }

        private string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        [DllImport("VendorCodeParser.dll", EntryPoint = "ParseVendorCode", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string ParseVendorCode(string str); // parse vendor code from name

        public bool GetObjectPropertiesFromCSV(ref IProduct pr, ref string line) // get product data from original CSV file
        {
            try
            {
                string[] lines = line.Replace("\"", "").Split('|');
                pr.Name = lines[0];
                if (lines[1].Length > 0)
                    pr.CompCode = lines[1];
                pr.VendorCode = ParseVendorCode(pr.Name);
                lines[2] = RemoveWhitespace(lines[2]);
                pr.Price = String.IsNullOrEmpty(lines[2]) ? 0 : Convert.ToDouble(lines[2]);
                if (pr.VendorCode.Length < 3/* || pr.Price == 0*/)
                    return false;
            }
            catch (Exception ex)
            { MessageBox.Show("Ошибка DLL: " + ex.Message); }

            return true;
        }

        public void UpdateConfig(bool isEndOfFile)
        {
            string file = DocFolderPath + "config.ini";
            string text = File.ReadAllText(file);
            string NomenclatureString = text.Substring(0, Math.Max(text.IndexOf('\n'), 0)); // find the string with parsed file name
            if (isEndOfFile) // ...if EOF
            {
                text = text.Replace(NomenclatureString, "Nomenclature:");    // ...remove name of CSV file from config
                Counter = 1;
            }
            else
            { text = text.Replace(NomenclatureString, "Nomenclature:" + ParsedFile); } // ...else replace it with current parsed file name
            text = text.Replace(TextToReplace, "Counter:" + Counter);
            File.WriteAllText(file, text);    
        }

        private string FindName(ref IWebDriver driver)
        {
            string name = string.Empty;
            //try
            //{
            //    name = driver.FindElement(By.CssSelector(SelectorName)).GetAttribute(AttributeName); // get the product name
            //}
            //catch 
            //{
            //    try
            //    {
            //        name = driver.FindElement(By.Id(SelectorName)).GetAttribute(AttributeName);
            //    }
            //    catch { }
            //}

            try
            {
                name = driver.FindElement(By.XPath(SelectorName)).Text;
            }
            catch { }

            return name;
        }

        private double FindPrice(ref IWebDriver driver)
        {
            string price = string.Empty;
            //try
            //{
            //    price = driver.FindElement(By.CssSelector(SelectorPrice)).GetAttribute(AttributeName); // get the product name
            //}
            //catch
            //{
            //    try
            //    {
            //        price = driver.FindElement(By.CssSelector(SelectorPrice)).Text;
            //    }
            //    catch { }
            //}

            try
            {
                price = driver.FindElement(By.XPath(SelectorPrice)).Text;
            }
            catch { }

            Regex rgx = new Regex(@"[^\d.,]");
            price = rgx.Replace(price, "");
            return Convert.ToDouble(price);
        }

        public void TryToParse(ref IWebDriver driver, ref IProduct pr)
        {
            try
            {
                var selector = driver.FindElements(By.XPath(SelectorTitle)); // try to find the specific product by xpath
                if (selector.Count != 0) // if this tag exists, then possibly the product we are looking for exists
                {
                    try
                    {
                        selector[0].Click(); // click it to open the product page
                        pr.OthName = FindName(ref driver); // get the product name
                        pr.OthName = pr.OthName.Trim();
                        pr.URL = driver.Url;
                        pr.OthPrice = FindPrice(ref driver); // get the price
                        pr.PriceDiff = pr.Price - pr.OthPrice;
                        if (pr.PriceDiff <= 0) // light red if the other price is less than company price
                            pr.IsPriceLess = true;
                        products.Add(pr);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Ошибка парсинга: " + ex.Message); }

        }

        public List<IProduct> StartParsing(string FileFromDialog)
        {
            if (IsFileCorrect())
            {
                IWebDriver driver = new ChromeDriver();

                if (FileFromDialog.Length > 0)
                {
                    ParsedFile = FileFromDialog;
                    Counter = 1;
                }

                StreamReader sr = new StreamReader(ParsedFile, Encoding.UTF8);
                int endID = Counter + AmountOfFiles; // calculate the end of parsing iteration (ending ID)
                string currentLine = SetFileStartPosition(ref sr);
                try
                {
                    while (!sr.EndOfStream && Counter < endID)
                    {
                        currentLine = sr.ReadLine();
                        IProduct product = new Product();
                        if (!GetObjectPropertiesFromCSV(ref product, ref currentLine))  // if data from CSV is incorrect, skip the product
                            continue;
                        Random rnd = new Random();
                        driver.Navigate().GoToUrl(ParsedLink + product.VendorCode); // this parser uses a searching link
                        Thread.Sleep(rnd.Next(1000, 4000)); // set random delay to avoid ban and jeopardy of possible DDOS
                        TryToParse(ref driver, ref product);
                        Thread.Sleep(rnd.Next(2000, 5000));
                        Counter++;
                    }
                }
                catch { }
                bool isEndOfFile = sr.Peek() == -1;
                driver.Quit();
                sr.Close();
                UpdateConfig(isEndOfFile);
            }
            else
            {
                MessageBox.Show("Номенклатура не получена или некорректно заполнен config.ini");
            }

            return products;
        }

        private string SetFileStartPosition(ref StreamReader sr)  // find the starting ID (the end of previous iteration)
        {
            string line = String.Empty;
            try
            {
                int index = 0;
                while (sr.Peek() != -1)
                {
                    line = sr.ReadLine();
                    if (index == Counter - 1)
                        return line;
                    index++;
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Не удалось найти начало файла для текущей итерации: " + ex.Message); }
            return line;
        }
    }
}
