using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

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
            bool res = parser.InitializeProperties();
            if (res)
            {
                FileBrowserBtn.IsEnabled = false;
                FileBrowserBtn.Visibility = Visibility.Hidden;
                FileBrowser.IsEnabled = false;
                FileBrowser.Visibility = Visibility.Hidden;
            }
            else
            { MessageBox.Show("Загрузите номенклатуру"); }
        }

        private void StartParsingBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultGrid.ItemsSource = parser.StartParsing(FilePath);
            MessageBox.Show("Процедура парсинга закончена, таблица выведена (при условии корректности данных). Для запуска следующей процедуры приложение надо обязательно перезагрузить (закрыть-открыть)");
            DisableControls();
        }

        private void DisableControls()
        {
            StartParsingBtn.IsEnabled = false;
            StartParsingBtn.Visibility = Visibility.Hidden;
            FileBrowserBtn.IsEnabled = false;
            FileBrowserBtn.Visibility = Visibility.Hidden;
            FileBrowser.IsEnabled = false;
            FileBrowser.Visibility = Visibility.Hidden;
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var destination = ((Hyperlink)e.OriginalSource).NavigateUri;

            using (Process browser = new Process())
            {
                browser.StartInfo = new ProcessStartInfo
                {
                    FileName = destination.ToString(),
                    UseShellExecute = true,
                    ErrorDialog = true
                };
                browser.Start();
            }
        }
    }
}
