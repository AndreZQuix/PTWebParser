using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

namespace PTWebParser
{
    public class WebParser : IWebParser
    {
        List<IProduct> products = new List<IProduct>();

        public string DocFolderPath = "../../../../Docs/";
        public static int AmountOfFiles = 10;
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
                    {
                        ParsedFile = line.Replace("Nomenclature:", "");
                        if (ParsedFile.Length == 0)
                            MessageBox.Show("Загрузите номенклатуру");
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
        }

        public bool IsFileCorrect()
        {
            return !string.IsNullOrEmpty(TextToReplace) && !string.IsNullOrEmpty(ParsedLink) && !string.IsNullOrEmpty(SelectorTitle) 
                && !string.IsNullOrEmpty(SelectorPrice) && !string.IsNullOrEmpty(SelectorName);
        }

        [DllImport("VendorCodeParser.dll", EntryPoint = "ParseVendorCode", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string ParseVendorCode(string str);
        private void ParseVendorCode(ref IProduct pr) // parse vendor code from name
        {
            pr.VendorCode = ParseVendorCode(pr.Name);
            pr.OthName = pr.VendorCode;
        }
        private string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public void GetObjectPropertiesFromCSV(ref IProduct pr, ref string line) // get product data from original CSV file
        {
            string[] lines = line.Replace("\"", "").Split('|');
            pr.ID = Convert.ToInt32(lines[0]);
            pr.Name = lines[1];

            if(lines[2].Length > 0)
                pr.CompCode = lines[2];

            ParseVendorCode(ref pr);

            lines[3] = RemoveWhitespace(lines[3]);
            if (!String.IsNullOrWhiteSpace(lines[3]))
            { 
                pr.Price = Convert.ToDouble(lines[3]);
            }
            else
            {
                pr.Price = 0;
            }
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
            {
                text = text.Replace(NomenclatureString, "Nomenclature:" + ParsedFile); // ...else replace it with current parsed file name
            }
            text = text.Replace(TextToReplace, "Counter:" + Counter);
            File.WriteAllText(file, text);    
        }

        public void TryToParse(ref IWebDriver driver, ref IProduct pr)
        {
            try
            {
                var selector = driver.FindElements(By.ClassName(SelectorTitle)); // try to find the specific product tag (tag that contains product name)
                if (selector.Count != 0) // if this tag exists, then possibly the right product we are looking for exists
                {
                    try
                    {
                        selector[0].Click(); // click it to open the product page
                        pr.OthName = driver.FindElement(By.CssSelector(SelectorName)).GetAttribute(AttributeName); // get the product name
                        pr.OthName = pr.OthName.Trim();

                        string price = driver.FindElement(By.CssSelector(SelectorPrice)).GetAttribute(AttributeName); // get the price
                        pr.OthPrice = Convert.ToDouble(price.Remove(price.Length - 1).Replace(" ", ""));
                        pr.PriceDiff = pr.Price - pr.OthPrice;
                        if (pr.PriceDiff <= 0) // light red if the other price is less than company price
                            pr.IsPriceLess = true;
                        products.Add(pr);
                    }
                    catch (Exception ex) { }
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

                StreamReader sr = new StreamReader(ParsedFile);
                int endID = Counter + AmountOfFiles; // calculate the end of parsing iteration (ending ID)
                string currentLine = SetFileStartPosition(ref sr);
                while((currentLine = sr.ReadLine()) != null && Counter < endID)
                {
                    IProduct product = new Product();
                    GetObjectPropertiesFromCSV(ref product, ref currentLine);
                    Random rnd = new Random();
                    driver.Navigate().GoToUrl(ParsedLink + product.VendorCode); // this parser uses a searching link
                    Thread.Sleep(rnd.Next(1000, 4000)); // set random delay to avoid ban and jeopardy of possible DDOS
                    TryToParse(ref driver, ref product);
                    Thread.Sleep(rnd.Next(2000, 5000));
                    Counter++;
                }
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
