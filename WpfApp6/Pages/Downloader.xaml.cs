using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfApp6.Services;
using System.Windows.Forms;
using SharpCompress.Archives; // Add this using directive for ZipArchive
using SharpCompress.Archives.Rar; // Add this using directive for RarArchive
using Newtonsoft.Json.Linq;
using System.IO.Abstractions;

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
                    string apiUrl = "http://127.0.0.1:5000/api/download"; // Replace with your API endpoint

                    // Set the flag indicating that download is in progress
                    downloadStateService.IsDownloadInProgress = true;

                    try
                    {
                        // Reset progress bar
                        DownloadProgressBar.Value = 0;
                        downloadStateService.ResetProgress();

                        // Fetch JSON data from the API endpoint
                        string jsonContent = await DownloadJsonContentAsync(apiUrl);

                        // Parse JSON to get the download URL
                        string downloadUrl = ParseDownloadUrl(jsonContent);

                        // Extract the file name from the URL
                        string fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);

                        // Download and extract a file from the obtained URL asynchronously
                        await DownloadAndExtractAsync(downloadPath, downloadUrl, fileName);

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

        private async Task<string> DownloadJsonContentAsync(string apiUrl)
        {
            // Check if apiUrl is a valid absolute URI
            if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var uri))
            {
                Console.WriteLine($"Invalid API URL: {apiUrl}");
                return null;
            }

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10); // Adjust the timeout as needed

                // Use the absolute URI for the API endpoint
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                // Log the response content
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content: " + responseContent);

                return responseContent;
            }
        }

        private string ParseDownloadUrl(string jsonContent)
        {
            try
            {
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);
                var downloadUrl = jsonObject.downloadurl[0].downloadurl.ToString();
                return downloadUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing download URL: {ex.Message}");
                return null; // Handle the error gracefully or throw an exception as needed
            }
        }

        private async Task DownloadAndExtractAsync(string downloadPath, string fileUrl, string fileName, int maxRetries = 3)
        {
            int retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    string downloadedFilePath = Path.Combine(downloadPath, fileName);

                    await DownloadFile(downloadPath, fileUrl);

                    // After successful download, introduce a delay before extraction and deletion
                    await Task.Delay(1000); // Add a delay of 1 second (adjust as needed)

                    try
                    {
                        // Determine the file format based on the extension
                        string extension = Path.GetExtension(downloadedFilePath)?.ToLowerInvariant();

                        using (IArchive archive = GetArchive(downloadedFilePath, extension))
                        {
                            if (archive != null)
                            {
                                ExtractEntries(archive, downloadPath);
                            }
                            else
                            {
                                // Handle unsupported file format or throw an exception
                                throw new NotSupportedException($"Unsupported file format: {extension}");
                            }
                        }

                        // Optionally, you can delete the downloaded file after extraction
                        File.Delete(downloadedFilePath);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception during extraction and deletion
                        throw;
                    }

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
        }



        private void ExtractEntriesFromZip(IArchive archive, string extractPath)
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    using (var entryStream = entry.OpenEntryStream())
                    {
                        var entryFilePath = Path.Combine(extractPath, entry.Key);
                        var entryDirectory = Path.GetDirectoryName(entryFilePath);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(entryDirectory))
                        {
                            Directory.CreateDirectory(entryDirectory);
                        }

                        using (var fileStream = new FileStream(entryFilePath, FileMode.Create))
                        {
                            entryStream.CopyTo(fileStream);
                        }
                    }
                }
            }
        }

        private void ExtractEntriesFromRar(IArchive archive, string extractPath)
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    using (var entryStream = entry.OpenEntryStream())
                    {
                        var entryFilePath = Path.Combine(extractPath, entry.Key);
                        var entryDirectory = Path.GetDirectoryName(entryFilePath);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(entryDirectory))
                        {
                            Directory.CreateDirectory(entryDirectory);
                        }

                        using (var fileStream = new FileStream(entryFilePath, FileMode.Create))
                        {
                            entryStream.CopyTo(fileStream);
                        }
                    }
                }
            }
        }

        private IArchive GetArchive(string filePath, string extension)
        {
            switch (extension)
            {
                case ".zip":
                    return SharpCompress.Archives.Zip.ZipArchive.Open(filePath);
                case ".rar":
                    return SharpCompress.Archives.Rar.RarArchive.Open(filePath);
                // Add more cases for other supported formats if needed
                default:
                    return null; // Unsupported format
            }
        }


        private async Task DownloadFile(string downloadPath, string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10); // Adjust the timeout as needed
                client.DefaultRequestHeaders.ConnectionClose = true;

                HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

                // Extract the file name from the URL
                string fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
                string downloadedFilePath = Path.Combine(downloadPath, fileName);

                long fileSize = response.Content.Headers.ContentLength.GetValueOrDefault();

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                              fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
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

        private void ExtractEntries(IArchive archive, string extractPath)
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    using (var entryStream = entry.OpenEntryStream())
                    {
                        var entryFilePath = Path.Combine(extractPath, entry.Key);
                        var entryDirectory = Path.GetDirectoryName(entryFilePath);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(entryDirectory))
                        {
                            Directory.CreateDirectory(entryDirectory);
                        }

                        using (var fileStream = new FileStream(entryFilePath, FileMode.Create))
                        {
                            entryStream.CopyTo(fileStream);
                        }
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
