using System.Windows;

namespace Images_Loader
{
    public partial class MainWindow : Window
    {
        private long _volumeToLoad = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartAll()
        {
            LoadingProgressBar.Value = 0;
            _volumeToLoad = 0;
            LoaderPanel1.onChunkDownloaded = b => PercentCounter(b);
            LoaderPanel2.onChunkDownloaded = b => PercentCounter(b);
            LoaderPanel3.onChunkDownloaded = b => PercentCounter(b);

            if (LoaderPanel1.UrlTextBox.Text != string.Empty)
            {
                var size = await LoaderPanel1.ImageByteCountAsync();
                _volumeToLoad += size.Value;
            }

            if (LoaderPanel2.UrlTextBox.Text != string.Empty)
            {
                var size = await LoaderPanel2.ImageByteCountAsync();
                _volumeToLoad += size.Value;
            }

            if (LoaderPanel3.UrlTextBox.Text != string.Empty)
            {
                var size = await LoaderPanel3.ImageByteCountAsync();
                _volumeToLoad += size.Value;
            }

            LoaderPanel1.StartLoading();
            LoaderPanel2.StartLoading();
            LoaderPanel3.StartLoading();
        }

        private void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            StartAll();
        }

        private void PercentCounter(long chunkSize)
        {
            double percent = (double)chunkSize / _volumeToLoad * 100;
            LoadingProgressBar.Value += percent;
        }
    }
}
