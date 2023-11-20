using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO.Compression;

namespace WpfApp6.Pages
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : System.Windows.Controls.UserControl
    {
        private CancellationTokenSource cancellationTokenSource;
        private FolderBrowserDialog folderBrowserDialog;

        public Downloader()
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
            folderBrowserDialog = new FolderBrowserDialog();
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ask where it should download to
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string downloadPath = folderBrowserDialog.SelectedPath;

                    // Enable cancel button
                    CancelButton.Visibility = Visibility.Visible;

                    try
                    {
                        // Reset progress bar
                        DownloadProgressBar.Value = 0;

                        // Download and extract a file from a website asynchronously
                        await DownloadAndExtractAsync(downloadPath, "https://cdn.blksservers.com/2.1.0.zip");

                        // For simplicity, I'll just show a message box
                        System.Windows.MessageBox.Show("File downloaded and extracted successfully!");
                    }
                    catch (OperationCanceledException)
                    {
                        System.Windows.MessageBox.Show("Download canceled!");

                        // Delete the partially downloaded file
                        if (Directory.Exists(downloadPath))
                        {
                            string partialDownloadPath = Path.Combine(downloadPath, "downloaded_file.zip");
                            if (File.Exists(partialDownloadPath))
                            {
                                File.Delete(partialDownloadPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error: {ex.Message}");
                    }
                    finally
                    {
                        // Disable cancel button
                        CancelButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task DownloadAndExtractAsync(string downloadPath, string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                long fileSize = response.Content.Headers.ContentLength.GetValueOrDefault();

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                              fileStream = new FileStream(Path.Combine(downloadPath, "downloaded_file.zip"), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        // Update progress bar on UI thread.
                        totalBytesRead += bytesRead;
                        int progressPercentage = (int)((double)totalBytesRead / fileSize * 100);
                        UpdateProgressBar(progressPercentage);
                    }
                }

                // Extract the downloaded file using System.IO.Compression
                ZipFile.ExtractToDirectory(Path.Combine(downloadPath, "downloaded_file.zip"), downloadPath);

                // Optionally, you can delete the downloaded zip file after extraction
                File.Delete(Path.Combine(downloadPath, "downloaded_file.zip"));
            }
        }

        private void UpdateProgressBar(int value)
        {
            // Ensure this method is called on the UI thread.
            Dispatcher.Invoke(() => DownloadProgressBar.Value = value);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the download operation
            cancellationTokenSource.Cancel();

            // Reset progress bar
            DownloadProgressBar.Value = 0;
        }
    }
}
