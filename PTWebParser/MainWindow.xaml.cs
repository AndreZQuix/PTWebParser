using Microsoft.Win32;
using System.Windows;

namespace PTWebParser
{
    public partial class MainWindow : Window
    {
        private IWebParser parser;

        public MainWindow()
        {
            InitializeComponent();

            parser = new WebParser();
            parser.InitializeProperties();
        }

        private void StartParsingBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultGrid.ItemsSource = parser.StartParsing();
            MessageBox.Show("Процедура парсинга закончена, таблица выведена. Для запуска следующей процедуры приложение надо обязательно перезагрузить (закрыть-открыть)");
            StartParsingBtn.IsEnabled = false;
        }

        private void FileBrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.Filter = "CSV files (*.csv)|*.csv";
            bool? Result = OpenFileDialog.ShowDialog();
        }
    }
}
