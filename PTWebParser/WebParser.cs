using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium;
using System.Threading.Tasks;

namespace PTWebParser
{
    public class WebParser : IWebParser
    {
        public string DocFolderPath = "../../../../Docs/";
        public static int AmountOfFiles = 10; // amount of files to parse at one iteration
        public static int Count = 0;

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

                    if(line.Contains("Count:"))
                    {
                        TextToReplace = line;
                        Count = Convert.ToInt32(line.Replace("Count:", ""));
                    }

                    if (line.Contains("AmountOfFiles:"))
                        AmountOfFiles = Convert.ToInt32(line.Replace("AmountOfFiles:", ""));

                    if (line.Contains("VendorCodes:"))
                        ParsedFile = line.Replace("VendorCodes:", "");

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

        public void GetObjectProperties(ref StreamReader sr, ref IProduct pr)
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

        public void UpdateCounter()
        {
            string file = DocFolderPath + "config.ini";
            string text = File.ReadAllText(file);
            text = text.Replace(TextToReplace, "Count:" + Count);
            File.WriteAllText(file, text);
        }

        public async Task<List<IProduct>> StartParsing()
        {
            Task<List<IProduct>> list = Task.FromResult(new List<IProduct>());
            if (IsFileCorrect())
            {
                IWebDriver driver = new EdgeDriver();
                StreamReader sr = new StreamReader(DocFolderPath + ParsedFile);
                int endID = Count + AmountOfFiles; // calculate the end of parsing iteration
                SetFileStartPosition(ref sr);
                while(sr.Peek() != -1 && Count < endID)
                {
                    IProduct product = new Product();
                    GetObjectProperties(ref sr, ref product);

                    Random rnd = new Random();
                    driver.Navigate().GoToUrl(ParsedLink + product.VendorCode); // this parser uses a searching link
                    await Task.Delay(rnd.Next(2000, 5000)); // set random delay to avoid ban and jeopardy of possible DDOS
                    //Try to parse
                    await Task.Delay(rnd.Next(3000, 8000));
                    Count++;
                }
                driver.Quit();
                UpdateCounter();
            }
            else
            {
                MessageBox.Show("Номенклатура не получена или некорректно заполнен config.ini");
            }

            return await list;
        }

        private void SetFileStartPosition(ref StreamReader sr)  // find the starting ID (the end of previous iteration)
        {
            string lineID = "ID:" + Convert.ToString(Count);
            string line;
            try
            {
                while (sr.Peek() != -1)
                {
                    line = sr.ReadLine();
                    if (line.Equals(lineID))
                        return;
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Не удалось найти начало файла для текущей итерации: " + ex.Message); }
        }
    }
}
