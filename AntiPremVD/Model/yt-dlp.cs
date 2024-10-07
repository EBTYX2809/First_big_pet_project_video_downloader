using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode.Common;

namespace AntiPremVD.Model
{
    public static class yt_dlp
    {
        public static async Task<Video> GetVideoInfo(string videoUrl)
        {
            string tempJsonFile = Path.GetTempFileName();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp.exe",
                        Arguments = $"--dump-json {videoUrl}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // jsonOutput - lag with this link https://www.youtube.com/watch?v=7Ikf4tr1Iug&list=PL_vll1HrGmB9D_PHrYT9HDKaqNaDCTZVw&index=68
                string jsonOutput = await process.StandardOutput.ReadToEndAsync();
                string errorOutput = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit());

                if (process.ExitCode != 0)
                {
                    MessageBox.Show($"Error: {errorOutput}");
                    return null;
                }

                // Parse a JSON string into a JObject for convenient data access
                JObject videoInfo = JObject.Parse(jsonOutput);

                var Qualities = ParseQualities(jsonOutput);

                return new Video
                (
                     url: videoUrl,
                     title: videoInfo["title"]?.ToString(),
                     duration: TimeSpan.FromSeconds(videoInfo["duration"]?.ToObject<double>() ?? 0).ToString(@"hh\:mm\:ss"),
                     views: videoInfo["view_count"]?.ToObject<long>().ToString("N0", System.Globalization.CultureInfo.InvariantCulture),
                     previewImg: videoInfo["thumbnail"]?.ToString(),
                     downloadInfo: new DownloadItem(
                         type: "video",
                         qualities: Qualities,
                         qualityPanelList: new ObservableCollection<string>(Qualities.Keys.Skip(2)),
                         sizes: ParseSizes(jsonOutput, Qualities),
                         selectedQuality: Qualities.Keys.Max() 
                ));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public static async Task<Dictionary<string, string>> GetQualities(string url)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp.exe",
                        Arguments = $"--dump-json {url}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string jsonOutput = await process.StandardOutput.ReadToEndAsync();
                string errorOutput = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit());

                if (process.ExitCode != 0)
                {
                    MessageBox.Show($"Error: {errorOutput}", "Process Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                // Delegate json to additional method for parse
                return ParseQualities(jsonOutput);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private static Dictionary<string, string> ParseQualities(string jsonOutput)
        {
            var qualities = new Dictionary<string, string>();

            try
            {
                JObject videoInfo = JObject.Parse(jsonOutput);

                var formats = videoInfo["formats"]?.ToObject<JArray>();

                if (formats != null)
                {
                    foreach (var format in formats)
                    {
                        string formatId = format["format_id"]?.ToString();
                        string resolution = format["format_note"]?.ToString();
                        string extension = format["ext"]?.ToString();
                        string acodec = format["acodec"]?.ToString();
                        int? abr = (int?)format["abr"];

                        // Searching bitrates
                        if (!string.IsNullOrEmpty(acodec) && acodec == "opus" && !qualities.ContainsKey(resolution))
                        {
                            // For bitrate about 64k (example from 50k to 70k)
                            if (abr >= 50 && abr <= 70)
                            {
                                qualities["64Kbit/s"] = formatId;
                            }
                            // For bitrate about 128k (example from 100k to 140k)
                            else if (abr >= 100 && abr <= 140)
                            {
                                qualities["128Kbit/s"] = formatId;
                            }
                        }

                        if (!string.IsNullOrEmpty(resolution) && extension == "webm" && resolution.Contains("p") && !qualities.ContainsKey(resolution))
                        {
                            qualities[resolution] = formatId;
                        }
                        // Need for another sites
                        /*else if(!string.IsNullOrEmpty(resolution) && extension == "mp4" && resolution.Contains("p") && !qualities.ContainsKey(resolution))
                        {
                            qualities[resolution] = formatId;
                        }*/
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing JSON output: {ex.Message}", "Parsing Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return qualities;
        }

        private static Dictionary<string, string> ParseSizes(string jsonOutput, Dictionary<string, string> qualities)
        {
            var sizes = new Dictionary<string, string>();

            try
            {
                // Parse JSON with JObject
                JObject videoInfo = JObject.Parse(jsonOutput);

                var formats = videoInfo["formats"]?.ToObject<JArray>();

                if (formats != null)
                {
                    foreach (var format in formats)
                    {
                        foreach (var quality in qualities)
                        {
                            string formatId = format["format_id"]?.ToString();
                            long? size = (long?)format["filesize"] ?? (long?)format["filesize_approx"];

                            if (!string.IsNullOrEmpty(formatId) && formatId == quality.Value && !sizes.ContainsKey(formatId))
                            {
                                sizes[quality.Key] = SizeConverter.FormatFileSize((long)size);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing JSON output: {ex.Message}", "Parsing Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return sizes;
        }
    }
}
