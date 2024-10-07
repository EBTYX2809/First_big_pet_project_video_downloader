using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace AntiPremVD.Model
{
    public class DownloadService
    {
        private string arguments = null;
        private Process process;
        private Video video = null;
        private double lastKnownProgress = 0; // For a smooth progress bar
        private bool isManuallyPaused = false; // For correct pauses
        private string tempFilePath; // For deleting temp yt-dlp files

        public DownloadService(Video video, string outputPath)
        {
            this.video = video;

            // For tempPath get name and extention
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputPath);
            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (video.DownloadInfo.Type == "audio")
            {
                arguments = $"-x \"{video.DownloadInfo.Qualities[video.DownloadInfo.SelectedQuality]}\" -o \"{outputPath}\" --progress --embed-thumbnail {video.URL}";
                tempFilePath = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}.x{video.DownloadInfo.Qualities[video.DownloadInfo.SelectedQuality]}.{video.DownloadInfo.Format}.part");
            }
            else if (video.DownloadInfo.Type == "video")
            {
                arguments = $"-f \"{video.DownloadInfo.Qualities[video.DownloadInfo.SelectedQuality]}+bestaudio\" -o \"{outputPath}\" --progress {video.URL}";
                tempFilePath = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}.f{video.DownloadInfo.Qualities[video.DownloadInfo.SelectedQuality]}.{video.DownloadInfo.Format}.part");
            }
        }

        public async Task StartDownload()
        {
            try
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp.exe",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                // Transferring progress
                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null && args.Data.Contains("%"))
                    {
                        string[] columns = args.Data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        double progress = double.Parse(columns[1].TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture);

                        if (progress >= lastKnownProgress)
                        {
                            // Update progress if the new value is greater than the previous one
                            lastKnownProgress = progress;
                            video.DownloadInfo.Progress = progress;
                        }
                    }
                };

                process.Start();
                process.BeginOutputReadLine();

                string errorOutput = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process?.WaitForExit());

                if (process?.ExitCode != 0 && !isManuallyPaused)
                {
                    MessageBox.Show($"Error: {errorOutput}");
                    CleanUpTempFiles();      
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
                return;
            }
        }

        public void PauseDownload()
        {
            if (process != null && !process.HasExited)
            {
                isManuallyPaused = true;
                KillProcessAndChildren(process.Id);
            }
        }

        public async Task ResumeDownload()
        {            
            await StartDownload();
            isManuallyPaused = false;
        }

        public void CancelDownload()
        {
             KillProcessAndChildren(process.Id);
             process = null;
             CleanUpTempFiles();
        }

        private void CleanUpTempFiles()
        {
            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed delete temp file: {ex.Message}");
            }
        }

        private void KillProcessAndChildren(int pid)
        {
            // Search every child process with WMI
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                $"Select * From Win32_Process Where ParentProcessId={pid}");

            ManagementObjectCollection objects = searcher.Get();

            foreach (ManagementObject obj in objects)
            {
                // Kill children processes with recursion
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessId"]));
            }

            // Kill main process
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException) {}
        }
    }
}
