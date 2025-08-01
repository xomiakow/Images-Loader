using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Images_Loader
{
    public partial class LoaderPanel : UserControl
    {
        private const int BufferSize = 8192;

        public Action<long>? onChunkDownloaded;
        private CancellationTokenSource? _cancellationToken;

        public LoaderPanel()
        {
            InitializeComponent();
        }

        public async void StartLoading()
        {
            _cancellationToken = new CancellationTokenSource();
            ClearLoader();

            if (UrlTextBox.Text != string.Empty)
            {
                try
                {
                    using var client = new HttpClient();
                    using var response = await client.GetAsync(UrlTextBox.Text, HttpCompletionOption.ResponseHeadersRead, _cancellationToken.Token);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(_cancellationToken.Token);
                    var buffer = new byte[BufferSize];
                    using var memoryStream = new MemoryStream();

                    var read = 0;
                    while ((read = await stream.ReadAsync(buffer, _cancellationToken.Token)) > 0)
                    {
                        await memoryStream.WriteAsync(buffer, 0, read, _cancellationToken.Token);
                        onChunkDownloaded?.Invoke(read);
                    }

                    memoryStream.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    ImageDisplay.Source = bitmap;
                }
                catch (OperationCanceledException)
                {
                    ErrorLabel.Content = "Загрузка отменена";
                }
                catch (Exception)
                {
                    ErrorLabel.Content = "Ошибка загрузки изображения";
                }
            }
        }

        public async Task<long?> ImageByteCountAsync()
        {
            try
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Head, UrlTextBox.Text);
                using var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                return response.Content.Headers.ContentLength;
            }
            catch
            {
                return null;
            }
        }

        private void ClearLoader() 
        {
            ErrorLabel.Content = "";
            ImageDisplay.Source = null;
        }

        private void ClearUrl()
        {
            UrlTextBox.Text = "";
        }

        private void StopLoading() 
        {
            _cancellationToken?.Cancel();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartLoading();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopLoading();
        }

        private void ClearUrlButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUrl();
        }
    }
}
