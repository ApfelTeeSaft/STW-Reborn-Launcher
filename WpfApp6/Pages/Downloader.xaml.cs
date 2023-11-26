using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp6.Services;
using System.Windows.Forms;
using System.IO.Compression;
using System.Net;
using SharpCompress.Archives;
using SharpZipArchive = SharpCompress.Archives.Zip.ZipArchive;
using SharpCompress.Common;

namespace WpfApp6.Pages
{
    public partial class Downloader : System.Windows.Controls.UserControl
    {
        private CancellationTokenSource cancellationTokenSource;
        private FolderBrowserDialog folderBrowserDialog;
        private DownloadStateService downloadStateService;

        public Downloader()
        {
            InitializeComponent();
            // Retrieve DownloadStateService instance from App resources
            downloadStateService = (DownloadStateService)App.Current.Resources["DownloadStateService"];
            cancellationTokenSource = new CancellationTokenSource();
            folderBrowserDialog = new FolderBrowserDialog();

            // Subscribe to progress changes
            downloadStateService.ProgressChanged += UpdateProgressBar;
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

                    // Set the flag indicating that download is in progress
                    downloadStateService.IsDownloadInProgress = true;

                    try
                    {
                        // Reset progress bar
                        DownloadProgressBar.Value = 0;
                        downloadStateService.ResetProgress();

                        // Download and extract a file from a website asynchronously
                        await DownloadAndExtractAsync(downloadPath, "https://cdn.blksservers.com/7.30.zip");

                        // For simplicity, I'll just show a message box
                        System.Windows.MessageBox.Show("File downloaded and extracted successfully!");
                    }
                    catch (OperationCanceledException)
                    {
                        System.Windows.MessageBox.Show("Download canceled!");

                        // Delete the partially downloaded file, removed cancel button since it was causing problems lol
                        if (Directory.Exists(downloadPath))
                        {
                            string partialDownloadPath = Path.Combine(downloadPath, "fortnite.rar");
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

                        // Reset the flag indicating that download is no longer in progress
                        downloadStateService.IsDownloadInProgress = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task DownloadAndExtractAsync(string downloadPath, string fileUrl, int maxRetries = 3)
        {
            int retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    await DownloadFile(downloadPath, fileUrl);
                    break; // Success, exit the loop
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    retries++;
                    if (retries == maxRetries)
                    {
                        // Throw an exception or handle failure after max retries
                        throw;
                    }
                }
            }

            // After successful download, introduce a delay before extraction and deletion
            await Task.Delay(1000); // Add a delay of 1 second (adjust as needed)

            try
            {
                using (var archive = SharpZipArchive.Open(Path.Combine(downloadPath, "fortnite.rar")))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(downloadPath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }

                // Optionally, you can delete the downloaded zip file after extraction
                File.Delete(Path.Combine(downloadPath, "fortnite.rar"));
            }
            catch (Exception ex)
            {
                // Log or handle the exception during extraction and deletion
                throw;
            }
        }

        private async Task DownloadFile(string downloadPath, string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10); // Adjust the timeout as needed
                client.DefaultRequestHeaders.ConnectionClose = true;

                HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

                long fileSize = response.Content.Headers.ContentLength.GetValueOrDefault();

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                              fileStream = new FileStream(Path.Combine(downloadPath, "fortnite.rar"), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
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
                        downloadStateService.DownloadProgress = progressPercentage;
                    }
                }
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
            downloadStateService.ResetProgress();

            // Reset the flag indicating that download is no longer in progress
            downloadStateService.IsDownloadInProgress = false;

            // Set the visibility state of the CancelButton in the service to false
            downloadStateService.IsCancelButtonVisible = false;
        }

    }
}