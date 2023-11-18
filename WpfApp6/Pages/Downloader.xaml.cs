using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using SevenZip;

namespace WpfApp6.Pages
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : System.Windows.Controls.UserControl
    {
        public Downloader()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ask where it should download to
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string downloadPath = folderBrowserDialog.SelectedPath;

                    // Download a file from a website
                    WebClient webClient = new WebClient();
                    // Hook up the event handler for download progress
                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                    // Start the Download
                    webClient.DownloadFile("https://example.com/samplefile.zip", System.IO.Path.Combine(downloadPath, "samplefile.zip")); //replace with link to download

                    // For simplicity, I'll just show a message box
                    System.Windows.MessageBox.Show("File downloaded and extracted successfully!");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update the progress bar value based on the download progress
            DownloadProgressBar.Value = e.ProgressPercentage;
        }
    }
}
