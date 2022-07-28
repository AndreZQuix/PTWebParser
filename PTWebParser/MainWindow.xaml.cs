using Microsoft.Win32;
using System.Windows;

namespace PTWebParser
{
    public partial class MainWindow : Window
    {
        private IWebParser parser;
        private string FilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            parser = new WebParser();
            parser.InitializeProperties();
        }

        private void StartParsingBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultGrid.ItemsSource = parser.StartParsing(FilePath);
            MessageBox.Show("Процедура парсинга закончена, таблица выведена (при условии корректности данных). Для запуска следующей процедуры приложение надо обязательно перезагрузить (закрыть-открыть)");
            StartParsingBtn.IsEnabled = false;
        }

        private void FileBrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.Filter = "CSV files (*.csv)|*.csv";
            if(OpenFileDialog.ShowDialog() == true)
            {
                FilePath = OpenFileDialog.FileName;
                FileBrowser.Text = FilePath;
            }
        }
    }
}
