using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AutoRefreshRate.Util
{
    class FileHandler
    {

        public void getFile(string appPath)
        {
            string filePath = appPath + @"/output.txt"; // Đường dẫn tới tệp dữ liệu
                                                        //  string outputFilePath = "D:\\output_file.txt";
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                string pattern = @"@ (\d+) Hz";

                HashSet<string> uniqueFrequencies = new HashSet<string>();

                foreach (string line in lines)
                {
                    MatchCollection matches = Regex.Matches(line, pattern);
                    foreach (Match match in matches)
                    {
                        string frequency = match.Groups[1].Value;
                        uniqueFrequencies.Add(frequency);
                    }

                }
                string[] myArray = uniqueFrequencies.ToArray();
                setArrayAppConfig("display", myArray);
                File.Delete(filePath);
            }
            else
            {
                Debug.WriteLine("Không tìm thấy tệp.");
            }
        }
        public void setArrayAppConfig(string key, string[] array)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


                if (config.AppSettings.Settings[key] == null)
                {

                    config.AppSettings.Settings.Add(key, string.Join(",", array));
                }
                else
                {

                    config.AppSettings.Settings[key].Value = string.Join(",", array);
                }


                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (ConfigurationErrorsException ex)
            {

                Console.WriteLine("Error writing to app config: " + ex.Message);
            }
        }
        public void setAppConfig(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        public void setFileDisplay(string appPath)
        {

            string filePath = appPath + @"/output.txt";

            // Kết hợp với tên thư mục Qures
            string qresDirectory = Path.Combine(appPath, "ThirdPartyLibraries");
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = qresDirectory,
                Arguments = "@\"/C QRes.exe -L",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            // MessageBox.Show("check run");
            // Debug.WriteLine("location :" + qresDirectory);
            process.StartInfo = startInfo;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string outputFilePath = filePath;
            File.WriteAllText(outputFilePath, output);
        }
        public bool getBooleanSetting(string key)
        {
            // Chuyển đổi giá trị từ chuỗi sang kiểu bool
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out bool result))
            {
                return result;
            }
            return false;
        }
        public async Task writeLog(string filePath, string message)
        {
           
                long fileSize = GetFileSize(filePath);
                int maxFileSizeInBytes = 1024 * 1024; // 1 MB

                if (fileSize > maxFileSizeInBytes)
                {
                    await TrimLogFileAsync(filePath);
                }
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                await writer.WriteLineAsync($"{DateTime.Now} : {message}");
                await writer.FlushAsync();
            }
        }
        static long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new FileInfo(filePath).Length;
            }
            return 0;
        }

        static async Task TrimLogFileAsync(string filePath)
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);

            int numLinesToKeep = (int)(lines.Length * 0.8);

            string[] newLines = new string[numLinesToKeep];
            Array.Copy(lines, lines.Length - numLinesToKeep, newLines, 0, numLinesToKeep);

            await File.WriteAllLinesAsync(filePath, newLines);
        }

    }
}
