using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;

namespace PTWebParser
{
    public class WebParser : IWebParser
    {
        List<IProduct> products = new List<IProduct>();

        public string DocFolderPath = "../../../../Docs/";
        public static int AmountOfFiles = 10; // amount of files to parse at one iteration
        public static int Counter = 1;

        private string ParsedLink = String.Empty;
        private string ParsedFile = String.Empty;
        private string SelectorTitle = String.Empty;
        private string SelectorName = String.Empty; // CSS selector to parse product name from web page
        private string SelectorPrice = String.Empty; // CSS selector to parse other product price from web page
        private string AttributeName = "textContent";   // attribute name to get from CSS selector
        private string TextToReplace = String.Empty;

        public void InitializeProperties() // read config and get parsing values
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
                        ParsedFile = line.Replace("Nomenclature:", "");

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
        }

        public bool IsFileCorrect()
        {
            return !string.IsNullOrEmpty(TextToReplace) && !string.IsNullOrEmpty(ParsedLink) && !string.IsNullOrEmpty(ParsedFile)
                && !string.IsNullOrEmpty(SelectorTitle) && !string.IsNullOrEmpty(SelectorPrice) && !string.IsNullOrEmpty(SelectorName);
        }

        public void GetObjectPropertiesFromTXT(ref StreamReader sr, ref IProduct pr)
        {
            string line;
            while(!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
            {
                if (line.Contains("Name:"))
                    pr.Name = line.Replace("Name:", "");

                if (line.Contains("CompID:"))
                    pr.CompCode = line.Replace("CompID:", "");

                if (line.Contains("Price:"))
                    pr.Price = Convert.ToDouble(line.Replace("Price:", ""));

                if (line.Contains("VendorCode:"))
                    pr.VendorCode = line.Replace("VendorCode:", "");
            }
        }

        private void ParseVendorCode(ref IProduct pr)
        {
            pr.VendorCode = "Hello world";
        }
        private string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public void GetObjectPropertiesFromCSV(ref StreamReader sr, ref IProduct pr, ref string line)
        {
            string[] lines = line.Split('|');
            pr.ID = Convert.ToInt32(lines[0]);
            pr.Name = lines[1];

            if(lines[2].Length > 0)
                pr.CompCode = lines[2];

            ParseVendorCode(ref pr);

            if (lines[3].Length > 0)
            {
                lines[3] = RemoveWhitespace(lines[3]);
                pr.Price = Convert.ToDouble(lines[3]);
            }

            line = sr.ReadLine();
        }

        public void UpdateCounter()
        {
            string file = DocFolderPath + "config.ini";
            string text = File.ReadAllText(file);
            text = text.Replace(TextToReplace, "Counter:" + Counter);
            File.WriteAllText(file, text);
        }

        public void TryToParse(ref IWebDriver driver, ref IProduct pr)
        {
            var selector = driver.FindElements(By.ClassName(SelectorTitle)); // try to find the specific product tag (tag that contains product name)
            if(selector.Count != 0) // if this tag exists, then possibly the right product we are looking for exists
            {
                selector[0].Click(); // click it to open the product page
                try
                {
                    pr.OthName = driver.FindElement(By.CssSelector(SelectorName)).GetAttribute(AttributeName); // get the product name
                    pr.OthName = pr.OthName.Trim();
                }
                catch (Exception ex)
                { MessageBox.Show("Ошибка парсинга названия " + pr.VendorCode + ": " + ex.Message); }

                try
                {
                    string price = driver.FindElement(By.CssSelector(SelectorPrice)).GetAttribute(AttributeName); // get the price
                    pr.OthPrice = Convert.ToDouble(price.Remove(price.Length - 1).Replace(" ", ""));
                    pr.PriceDiff = pr.Price - pr.OthPrice;
                    if (pr.PriceDiff <= 0) // light red if the other price is less than company price
                        pr.IsPriceLess = true;
                }
                catch(Exception ex)
                { MessageBox.Show("Ошибка парсинга стоимости " + pr.Name + ": " + ex.Message); }

                products.Add(pr);
            }
        }

        public List<IProduct> StartParsing()
        {
            if (IsFileCorrect())
            {
                IWebDriver driver = new ChromeDriver();
                StreamReader sr = new StreamReader(DocFolderPath + ParsedFile);
                int endID = Counter + AmountOfFiles; // calculate the end of parsing iteration
                string currentLine = SetFileStartPosition(ref sr);
                while(sr.Peek() != -1 && Counter <= endID)
                {
                    IProduct product = new Product();
                    GetObjectPropertiesFromCSV(ref sr, ref product, ref currentLine);

                    Random rnd = new Random();
                    driver.Navigate().GoToUrl(ParsedLink + product.VendorCode); // this parser uses a searching link
                    Thread.Sleep(1000); // set random delay to avoid ban and jeopardy of possible DDOS
                    TryToParse(ref driver, ref product);
                    Thread.Sleep(1000);
                    products.Add(product);
                    Counter++;
                }
                driver.Quit();
                sr.Close();
                UpdateCounter();
            }
            else
            {
                MessageBox.Show("Номенклатура не получена или некорректно заполнен config.ini");
            }

            return products;
        }

        private string SetFileStartPosition(ref StreamReader sr)  // find the starting ID (the end of previous iteration)
        {
            string lineID = Convert.ToString(Counter);
            string line = String.Empty;
            try
            {
                while (sr.Peek() != -1)
                {
                    line = sr.ReadLine();
                    string[] lines = line.Split('|');
                    if (lines[0].Equals(lineID))
                        return line;
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Не удалось найти начало файла для текущей итерации: " + ex.Message); }
            return line;
        }
    }
}
