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
        private string SettingsPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            parser = new WebParser();
            bool ret = parser.InitializeConfig(); // return if config and settings exist
            if (ret)
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
            ResultGrid.ItemsSource = parser.StartParsing(FilePath, SettingsPath);
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

        private void SettingsBrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ini files (*.ini)|*.ini";
            if(ofd.ShowDialog() == true)
            {
                SettingsPath = ofd.FileName;
                SettingsBrowser.Text = SettingsPath;
            }
        }

        private void FileBrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV files (*.csv)|*.csv";
            if(ofd.ShowDialog() == true)
            {
                FilePath = ofd.FileName;
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
