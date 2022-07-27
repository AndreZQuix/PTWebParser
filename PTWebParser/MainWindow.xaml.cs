using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium;

namespace PTWebParser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            IWebParser parser = new WebParser();
            parser.InitializeProperties();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            

            IWebDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://prodteh.ru/");
        }
    }
}
