using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using WpfApp6.Services;
using WpfApp6.Services.Launch;

namespace WpfApp6.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        public Home()
        {
            InitializeComponent();
        }

        public class DataUtility
        {
            private static readonly string FileName = "data.json";

            public static void SaveData(string email, string password, string path)
            {
                var data = new DataModel { Email = email, Password = password, Path = path };
                string jsonData = JsonConvert.SerializeObject(data);

                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string LauncherFolderPath = Path.Combine(localAppDataPath, "Launcher");

                Directory.CreateDirectory(LauncherFolderPath);

                string filePath = Path.Combine(LauncherFolderPath, FileName);

                File.WriteAllText(filePath, jsonData);
            }

            public static DataModel LoadData()
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string LauncherFolderPath = Path.Combine(localAppDataPath, "Launcher");
                string filePath = Path.Combine(LauncherFolderPath, FileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        string jsonData = File.ReadAllText(filePath);
                        return JsonConvert.DeserializeObject<DataModel>(jsonData);
                    }
                    catch (Exception ex)
                    {
                        // Handle deserialization error
                        Console.WriteLine($"Error deserializing data: {ex.Message}");
                    }
                }

                return null;
            }

            public class DataModel
            {
                public string Email { get; set; }
                public string Password { get; set; }
                public string Path { get; set; }
            }
        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path69 = UpdateINI.ReadValue("Auth", "Path");
                if (path69 != "NONE")
                {
                    string exeFilePath = System.IO.Path.Join(path69, "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe");

                    // Check if the file exists and its version matches
                    if (File.Exists(exeFilePath) && IsFileVersionMatch(exeFilePath, new Version("4.22.0.0")))
                    {
                        if (UpdateINI.ReadValue("Auth", "Email") == "NONE" || UpdateINI.ReadValue("Auth", "Password") == "NONE")
                        {
                            MessageBox.Show("Please Add Your Launcher Info In Settings");
                            return;
                        }

                        WebClient OMG = new WebClient();
                        OMG.DownloadFile("https://cdn.discordapp.com/attachments/1122256554331226122/1178432513304166593/Curl.dll?ex=65761fcd&is=6563aacd&hm=20cde775a5dd544aaf51d26769db297ed521947583a499b83bfd165a1ae7846b&", Path.Combine(path69, "Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64", "GFSDK_Aftermath_Lib.x64.dll")); //replace with your curl
                        OMG.DownloadFile("https://cdn.discordapp.com/attachments/1122256554331226122/1178425958802403481/pakchunk6004-WindowsClient.pak?ex=657619b2&is=6563a4b2&hm=2798fa5f264b97743f880b415b4d655a8aac9780c014ae806e10d07fe63b6aa4&", Path.Combine(path69, "FortniteGame\\Content\\Paks", "pakchunk6004-WindowsClient.pak")); //replace with custom pak (current one is for 7.30 i made as a test
                        OMG. DownloadFile("https://cdn.discordapp.com/attachments/1122256554331226122/1178425959427342386/pakchunk6004-WindowsClient.sig?ex=657619b2&is=6563a4b2&hm=8a3174b67c5dc8388f9c3c808452a7126fe7efb617a6dfc118b2598866caa3ed&", Path.Combine(path69, "FortniteGame\\Content\\Paks", "pakchunk6004-WindowsClient.sig")); //replace with sig (current one is for 7.30 i made as a test)

                        PSBasics.Start(path69, "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck", UpdateINI.ReadValue("Auth", "Email"), UpdateINI.ReadValue("Auth", "Password"));

                        FakeAC.Start(path69, "FortniteClient-Win64-Shipping_BE.exe", $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck", "r");
                        FakeAC.Start(path69, "FortniteLauncher.exe", $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck", "dsf");

                        PSBasics._FortniteProcess.WaitForExit();

                        try
                        {
                            FakeAC._FNLauncherProcess.Close();
                            FakeAC._FNAntiCheatProcess.Close();
                            File.Delete(Path.Combine(path69, "FortniteGame\\Content\\Paks", "pakchunk6004-WindowsClient.pak")); //delete custom pak
                            File.Delete(Path.Combine(path69, "FortniteGame\\Content\\Paks", "pakchunk6004-WindowsClient.sig")); //delete sig file
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("There has been an error closing");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Either the file does not exist or the version is incorrect!\nVersion Required: 7.30");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("UNKNOWN ERROR");
            }
        }

        static bool IsFileVersionMatch(string filePath, Version expectedVersion)
        {
            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

                int fileMajorPart = fileVersionInfo.FileMajorPart;
                int fileMinorPart = fileVersionInfo.FileMinorPart;
                int fileBuildPart = fileVersionInfo.FileBuildPart;
                int filePrivatePart = fileVersionInfo.FilePrivatePart;

                return fileMajorPart == expectedVersion.Major &&
                       fileMinorPart == expectedVersion.Minor &&
                       fileBuildPart == expectedVersion.Build &&
                       filePrivatePart == expectedVersion.Revision;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
