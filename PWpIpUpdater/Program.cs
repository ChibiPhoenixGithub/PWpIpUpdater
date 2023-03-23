using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PWpIpUpdater
{
    internal class AppConfigModel
    {
        public string ClientPath { get; set; } = string.Empty;
    }

    internal enum LogLevel
    {
        Info,
        Warning,
        Error,
    }

    internal class Program
    {
#pragma warning disable 8618
        private static AppConfigModel m_config;
#pragma warning restore 8618
        private static string m_path = string.Empty;
        private static string m_serverlistPath = "patcher/server/serverlist.txt";
        private static string m_updateserverPath = "patcher/server/updateserver.txt";
        private static string m_mainuniPath = "patcher/skin/mainuni.xml";

        private static readonly Regex m_regex = new Regex("((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)");

        private static void Main(string[] args)
        {
            m_config = GetConfig() ?? new AppConfigModel();
            LoadConfig();

            ConsoleKey userInput;
            bool keepRunning = true;
            do
            {
                DisplayWelcomeMessage();
                DisplayMenu();

                userInput = Console.ReadKey(true).Key;
                switch (userInput)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Console.Clear();
                        DisplayCurrentClientConfiguration();
                        PressAnyKeyToContinue();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Input the IPv4 Address you want to use for your client:");
                        Console.ResetColor();

                        var input = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            if (IsIpv4Address(input))
                            {
                                Console.WriteLine();
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Previous config:");
                                Console.ResetColor();
                                DisplayCurrentClientConfiguration();
                                Console.WriteLine();

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Updated to:");
                                Console.ResetColor();
                                UpdateClientConfiguration(input);
                                DisplayCurrentClientConfiguration();
                            }
                        }

                        PressAnyKeyToContinue();
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Console.Clear();
                        DisplayClientRootFolder();
                        PressAnyKeyToContinue();
                        break;
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        keepRunning = false;
                        break;
                    default:
                        Console.Clear();
                        break;                    
                }
            } while (keepRunning);
        }

        private static AppConfigModel? GetConfig()
        {
            AppConfigModel? output = null;

            var wat = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PWpIpUpdater.appconfig.json");

            if (stream != null)
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    var json = sr.ReadToEnd();
                    if (json != null)
                    {
                        output = JsonSerializer.Deserialize<AppConfigModel>(json);
                    }
                }
            }

            return output;
        }        

        private static void LoadConfig()
        {
            m_path = m_config.ClientPath;
            m_serverlistPath = Path.Combine(m_path, m_serverlistPath);
            m_updateserverPath = Path.Combine(m_path, m_updateserverPath);
            m_mainuniPath = Path.Combine(m_path, m_mainuniPath);
        }

        private static void PressAnyKeyToContinue()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
            Console.Clear();
        }

        private static void DisplayClientRootFolder()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("appconfig.json ");
            Console.ResetColor();
            Console.WriteLine("reports client root folder is located at:");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(m_config.ClientPath);
            Console.ResetColor();
        }

        private static void DisplayWelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Server Address Patcher by JJC Verhoeven");
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void DisplayMenu()
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("=================================");
            Console.WriteLine("1) Display current client configuration");
            Console.WriteLine("2) Update client IP configuration");
            Console.WriteLine("5) Display client root folder");
            Console.WriteLine("0) Exit");
        }

        private static void DisplayCurrentClientConfiguration()
        {
            DisplayServerlist();
            DisplayUpdateServer();
            DisplayMainuni();
        }

        private static void DisplayMainuni()
        {
            if (File.Exists(m_mainuniPath))
            {
                var lines = File.ReadAllLines(m_mainuniPath);
                var line = lines.FirstOrDefault(x => x.TrimStart().StartsWith("<SkinBrowser Name=\"UpdateBrowser\" MapColor=\"255,0,0\" InitURL="));
                if (line != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("mainuni.xml ");
                    Console.ResetColor();
                    Console.WriteLine("uses the following config:");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(line);
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
            else
            {
                WriteDebugMessage($"Failed to find 'mainuni.xml' at: { m_mainuniPath }", LogLevel.Error);
            }
        }

        private static void DisplayServerlist()
        {
            if (File.Exists(m_serverlistPath))
            {
                var lines = File.ReadAllLines(m_serverlistPath);
                var line = lines.FirstOrDefault(x => x.StartsWith("Land of Biting"));
                if (line != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("serverlist.txt ");
                    Console.ResetColor();
                    Console.WriteLine("uses the following config:");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(line);
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
            else
            {
                WriteDebugMessage($"Failed to find 'serverlist.txt' at: { m_serverlistPath }", LogLevel.Error);
            }
        }

        private static void DisplayUpdateServer()
        {
            if (File.Exists(m_updateserverPath))
            {
                var lines = File.ReadAllLines(m_updateserverPath);
                var line = lines.FirstOrDefault();
                if (line != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("updateserver.txt ");
                    Console.ResetColor();
                    Console.WriteLine("uses the following config:");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(line);
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
            else
            {
                WriteDebugMessage($"Failed to find 'updateserver.txt' at: { m_updateserverPath }", LogLevel.Error);
            }
        }

        private static bool IsIpv4Address(string address)
        {
            bool output = m_regex.IsMatch(address);

            if (output == false)
            {
                WriteDebugMessage($"'{ address }' - this is not a valid IPv4 address, a valid IPv4 Address looks like: xxx.xxx.xxx.xxx", LogLevel.Warning);
            }

            return output;
        }

        private static void UpdateClientConfiguration(string ipAddress)
        {
            UpdateServerlist(ipAddress);
            UpdateUpdateServer(ipAddress);
            UpdateMainuni(ipAddress);
        }

        private static void UpdateMainuni(string ipAddress)
        {
            if (File.Exists(m_mainuniPath))
            {
                var lines = File.ReadAllLines(m_mainuniPath);
                File.WriteAllLines(m_mainuniPath + ".bak", lines);

                var line = lines.First(x => x.TrimStart().StartsWith("<SkinBrowser Name=\"UpdateBrowser\" MapColor=\"255,0,0\" InitURL="));
                int index = lines.ToList().IndexOf(line);

                if (line != null)
                {
                    var replacement = m_regex.Replace(line, ipAddress);
                    lines[index] = replacement;

                    File.WriteAllLines(m_mainuniPath, lines);
                }
            }
            else
            {
                WriteDebugMessage($"Failed to find 'mainuni.xml' at: { m_mainuniPath }", LogLevel.Error);
            }
        }

        private static void UpdateServerlist(string ipAddress)
        {
            if (File.Exists(m_serverlistPath))
            {
                var lines = File.ReadAllLines(m_serverlistPath);
                File.WriteAllLines(m_serverlistPath + ".bak", lines);

                var line = lines.First(x => x.StartsWith("Land of Biting"));
                int index = lines.ToList().IndexOf(line);

                if (line != null)
                {
                    var replacement = m_regex.Replace(line, ipAddress);
                    lines[index] = replacement;

                    File.WriteAllLines(m_serverlistPath, lines);
                }
            }
            else
            {
                WriteDebugMessage($"Failed to find 'serverlist.txt' at: { m_serverlistPath }", LogLevel.Error);
            }
        }

        private static void UpdateUpdateServer(string ipAddress)
        {
            if (File.Exists(m_updateserverPath))
            {
                var lines = File.ReadAllLines(m_updateserverPath);
                File.WriteAllLines(m_updateserverPath + ".bak", lines);

                var line = lines.First();
                int index = lines.ToList().IndexOf(line);

                if (line != null)
                {
                    var replacement = m_regex.Replace(line, ipAddress);
                    lines[index] = replacement;

                    File.WriteAllLines(m_updateserverPath, lines);
                }
            }
            else
            {
                WriteDebugMessage($"Failed to find 'updateserver.txt' at: { m_updateserverPath }", LogLevel.Error);
            }
        }

        private static void WriteDebugMessage(string message, LogLevel logLevel = LogLevel.Info)
        {
            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.Cyan,
            };

            Console.WriteLine($"[{ logLevel }] - { message }");
            Console.ResetColor();
        }
    }
}