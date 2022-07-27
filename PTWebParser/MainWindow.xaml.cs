using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PTWebParser
{
    public partial class MainWindow : Window
    {
        private IWebParser parser;
        private List<IProduct> products = new List<IProduct>();

        public MainWindow()
        {
            InitializeComponent();

            parser = new WebParser();
            parser.InitializeProperties();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            parser.StartParsing();
        }
    }
}
