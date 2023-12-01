using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp6.Pages
{
    public partial class AnnouncementsPage : Page
    {
        public AnnouncementsPage()
        {
            InitializeComponent();
            LoadDataFromApi();
        }

        public async void LoadDataFromApi()
        {
            try
            {
                string apiUrl = "http://127.0.0.1:5000/api/announcements"; //replace with actual server url
                using (HttpClient client = new HttpClient())
                {
                    string jsonResult = await client.GetStringAsync(apiUrl);

                    JObject data = JObject.Parse(jsonResult);

                    if (data != null)
                    {
                        // Assuming "announcements" is the property containing the array of announcements
                        JArray announcementsArray = (JArray)data["announcements"];

                        if (announcementsArray != null)
                        {
                            // Deserialize the JSON array into a list of Announcement objects
                            List<Announcement> announcements = announcementsArray.Select(item => new Announcement
                            {
                                Author = item["author"].ToString(),
                                Avatar = item["avatar"].ToString(),
                                Message = item["message"].ToString(),
                                DateTime = DateTime.Parse(item["datetime"].ToString()) // Assuming the datetime field in JSON
                            }).ToList();

                            // Sort the announcements by DateTime in descending order (newest first)
                            announcements = announcements.OrderByDescending(a => a.DateTime).ToList();

                            // Display the sorted announcements
                            foreach (var announcement in announcements)
                            {
                                AddAnnouncementToUI(announcement.Author, announcement.Avatar, announcement.Message, announcement.DateTime);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAnnouncementToUI(string author, string avatarUrl, string message, DateTime dateTime)
        {
            // Assume dateTime is in UTC
            var utcTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            // Convert UTC time to local time
            var localTimeZone = TimeZoneInfo.Local;
            var localTimeConverted = TimeZoneInfo.ConvertTime(utcTime, localTimeZone);

            StackPanel announcementPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };

            Image avatarImage = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(avatarUrl)),
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 0, 10, 0)
            };

            TextBlock authorTextBlock = new TextBlock
            {
                Text = $"{author}",
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff00e2"))
            };

            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(10, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap // Set TextWrapping to Wrap
            };

            TextBlock dateTextBlock = new TextBlock
            {
                Text = $" ({localTimeConverted:yyyy-MM-dd HH:mm})",
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#87CEEB")) // Light blue color
            };

            announcementPanel.Children.Add(avatarImage);
            announcementPanel.Children.Add(authorTextBlock);
            announcementPanel.Children.Add(messageTextBlock);
            announcementPanel.Children.Add(dateTextBlock);

            AnnouncementsStackPanel.Children.Add(announcementPanel);
        }



        // Define a class to represent an Announcement
        public class Announcement
        {
            public string Author { get; set; }
            public string Avatar { get; set; }
            public string Message { get; set; }
            public DateTime DateTime { get; set; }
        }

        private void AnnouncementsStackPanel_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}