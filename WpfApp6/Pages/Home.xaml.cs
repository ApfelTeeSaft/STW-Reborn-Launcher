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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path69 = UpdateINI.ReadValue("Auth", "Path");
                if (path69 != "NONE")
                {
                    string exeFilePath = System.IO.Path.Join(path69, "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe");

                    // Check if the file exists and its version matches
                    if (File.Exists(exeFilePath) && IsFileVersionMatch(exeFilePath, new Version("4.21.0.0")))
                    {
                        if (UpdateINI.ReadValue("Auth", "Email") == "NONE" || UpdateINI.ReadValue("Auth", "Password") == "NONE")
                        {
                            MessageBox.Show("Please Add Your Project - STW Info In Settings");
                            return;
                        }

                        WebClient OMG = new WebClient();
                        OMG.DownloadFile("https://cdn.discordapp.com/attachments/1173024806015348818/1176212795742765156/Cobalt.dll?ex=656e0c87&is=655b9787&hm=99ba61512b4a882cbb33cfc505b99a0c4bffd49759bb132febf0121ac4bdfab7&", Path.Combine(path69, "Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64", "GFSDK_Aftermath_Lib.x64.dll")); //replace with your curl

                        PSBasics.Start(path69, "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck", UpdateINI.ReadValue("Auth", "Email"), UpdateINI.ReadValue("Auth", "Password"));

                        FakeAC.Start(path69, "FortniteClient-Win64-Shipping_BE.exe", $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck", "r");
                        FakeAC.Start(path69, "FortniteLauncher.exe", $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -noeac -fromfl=be -fltoken=h1cdhchd10150221h130eB56 -skippatchcheck", "dsf");

                        PSBasics._FortniteProcess.WaitForExit();

                        try
                        {
                            FakeAC._FNLauncherProcess.Close();
                            FakeAC._FNAntiCheatProcess.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("There has been an error closing");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Either the file does not exist or the version is incorrect!\nVersion Required: 2.1.0");
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
