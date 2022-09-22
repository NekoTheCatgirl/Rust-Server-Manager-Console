using System.IO.Compression;
using System.Diagnostics;
using Open.Nat;
using Newtonsoft.Json;

namespace RustManagerConsole;

class Program
{
    static MainSettings settings;

    static string AppDir = string.Empty;

    static void Main()
    {
        AppDir = Environment.CurrentDirectory;

        Console.WriteLine("Welcome to the rust console");
        Console.Title = "Rust Manager Console";
        // Main Config Handler
        if (File.Exists("Config.json"))
        {
            settings = JsonConvert.DeserializeObject<MainSettings>(File.ReadAllText("Config.json"));
        }
        else
        {
            FirstTimeSetup();
        }

        // Server Selection Handler
        string[] Servers = Array.Empty<string>();
        if (Directory.Exists("Servers"))
        {
            Servers = Directory.GetFiles("Servers", "*.json");
        }
        else
        {
            Directory.CreateDirectory("Servers");
        }

        if (Servers.Length > 0)
        {
            while (true)
            {
                if (ApplicationMenu(Servers)) break;
            }
        }
        else
        {
            Console.WriteLine("No servers were detected, initiating the creation process.");
            Thread.Sleep(500);
            var server = ServerObject.CreateNewServer();
            File.WriteAllText($"Servers/{server.Server.Identity}.json", JsonConvert.SerializeObject(server, Formatting.Indented));

            StartServer(server);
        }
    }

    static bool ApplicationMenu(string[] Servers)
    {
        string[] Options = new[] { "Update server files", "Create new server", "Start Existing Server", "Exit" };

        ConsoleExtension.CreateMenu($"Server manager options, {Servers.Length} servers detected.", Options, out int selection);

        if (selection == 0)
        {
            UpdateInstallation();

            return false;
        }
        else if (selection == 1)
        {
            var server = ServerObject.CreateNewServer();
            File.WriteAllText($"Servers/{server.Server.Identity}.json", JsonConvert.SerializeObject(server, Formatting.Indented));

            StartServer(server);

            return false;
        }
        else if (selection == 2)
        {
            List<string> ServersOptions = new();

            foreach (var s in Servers)
            {
                ServersOptions.Add(Path.GetFileNameWithoutExtension(s));
            }

            ConsoleExtension.CreateMenu("Select the server you want to start.", ServersOptions.ToArray(), out int serverSelection);

            var server = JsonConvert.DeserializeObject<ServerObject>(File.ReadAllText(Servers[serverSelection]));

            StartServer(server);

            return false;
        }
        return true;
    }

    static void FirstTimeSetup()
    {
        settings = new MainSettings();
        Console.WriteLine("Starting setup sequence!");
        Thread.Sleep(1000);
        ConsoleExtension.Propmpt("Do you want to let this app automatically install SteamCMD, and Rust dedicated? y/n", out string promptChoice, Validators.YesNoValidator);
        if (Converters.YesNoConvert(promptChoice))
        {
            Console.WriteLine("Beginning automatic setup now.");
            AutomaticSetupSequence();
            settings.ManuallyInstalled = false;
        }
        else
        {
            Console.WriteLine("Okay, manual setup selected...");
            Thread.Sleep(1000);
            ConsoleExtension.Propmpt("Please provide the path leading to the RustDedicated.exe file", out string RustDedicatedPath, input => { if (File.Exists(input)) return true; return false; }, 5);
            Console.Clear();
            settings.RustDedicatedPath = RustDedicatedPath;
            settings.ManuallyInstalled = true;
        }
        settings.AutoRestart = false;
        File.WriteAllText("Config.json", JsonConvert.SerializeObject(settings));
    }

    static void AutomaticSetupSequence()
    {
        // Download Steam CMD
        Console.WriteLine("Downloading SteamCMD...");
        bool scdDownRes = DownloadFile(ConstantValues.SteamCmdDownloadLink, "SteamCMD.zip");
        ErrorCheck(scdDownRes, "Steam cmd failed to download propperly");

        // Unzip Steam CMD
        Console.WriteLine("Unzipping SteamCMD...");
        bool scdUZRes = UnzipFile("SteamCMD.zip", "steamcmd");
        ErrorCheck(scdUZRes, "Failed to unzip Steam CMD");

        // Validate that the files got extracted
        Console.WriteLine("Validating SteamCMD install...");
        bool scdFSRes = File.Exists("steamcmd/steamcmd.exe");
        ErrorCheck(scdFSRes, "Extracted files are invalid");

        settings.SteamCMDPath = $@"{AppDir}\steamcmd\steamcmd.exe";

        Console.WriteLine("SteamCMD Successfully installed.");
        // Install rust dedicated
        Console.WriteLine("Installing Rust Dedicated now.");
        SteamCMDCommand($"+login anonymous +app_update {ConstantValues.RustDedicatedID} +quit");

        // Validate install
        Console.WriteLine("Validating Rust Dedicated install...");
        bool rsdFSRes = File.Exists(@"steamcmd\steamapps\common\rust_dedicated\RustDedicated.exe");
        ErrorCheck(rsdFSRes, "Rust dedicated failed to install");

        settings.RustDedicatedPath = $@"{AppDir}\steamcmd\steamapps\common\rust_dedicated\RustDedicated.exe";

        Console.WriteLine("Rust Dedicated Successfully installed.");

        // Oxide auto install prompt
        ConsoleExtension.Propmpt("Do you want to install oxide automatically whenever rust is updated? y/n", out string selection, Validators.YesNoValidator);
        if (Converters.YesNoConvert(selection))
        {
            settings.AutoOxide = true;
        }
        else
        {
            settings.AutoOxide = false;
        }

        // Oxide (optional)
        OxideInstall();
        // Cleanup installed files
        Console.WriteLine("Cleaning up...");
        File.Delete("SteamCMD.zip");
        Thread.Sleep(1000);
    }

    static void UpdateInstallation()
    {
        Console.WriteLine("Updating Rust Dedicated...");
        SteamCMDCommand($"+login anonymous +app_update {ConstantValues.RustDedicatedID} +quit");

        OxideInstall();
    }

    static void OxideInstall()
    {
        bool doInstall = false;
        // Prompt about oxide
        if (!settings.AutoOxide)
        {
            ConsoleExtension.Propmpt("Do you want to install oxide?", out string oxideChoice, Validators.YesNoValidator);
            if (Converters.YesNoConvert(oxideChoice))
            {
                doInstall = true;
            }
        }
        else doInstall = true;

        if (doInstall)
        {
            bool oxiDownRes = DownloadFile(ConstantValues.OxideDownloadLink, "Oxide.Rust.zip");
            ErrorCheck(oxiDownRes, "Oxide failed to download, skipping...", true);

            if (oxiDownRes)
            {
                bool oxiUzRes = UnzipFile("Oxide.Rust.zip", $@"{AppDir}\steamcmd\steamapps\common\rust_dedicated", true);
                ErrorCheck(oxiUzRes, "Oxide failed to extract", true);

                if (oxiUzRes)
                {
                    Console.WriteLine("Oxide installed correctly.");
                    Thread.Sleep(4000);
                }

                File.Delete("Oxide.Rust.zip");
            }
        }
    }

    static void SteamCMDCommand(string arguments)
    {
        var steamCMD = new Process();
        steamCMD.StartInfo.FileName = settings.SteamCMDPath;
        steamCMD.StartInfo.Arguments = arguments;
        steamCMD.StartInfo.UseShellExecute = true;
        steamCMD.StartInfo.CreateNoWindow = true;
        steamCMD.StartInfo.WorkingDirectory = Path.GetDirectoryName(settings.SteamCMDPath);

        steamCMD.Start();
        steamCMD.WaitForExit();
    }

    static void ErrorCheck(bool value, string message, bool doNotThrow = false)
    {
        if (value)
        {
            return;
        }
        Console.WriteLine("An error has occured.\nMsg: {0}", message);
        Thread.Sleep(5000);
        if (!doNotThrow)
            throw new Exception();
    }

    static bool DownloadFile(string uri, string file)
    {
        try
        {
            using var client = new HttpClient();
            using var stream = client.GetStreamAsync(uri);
            using var fs = new FileStream(file, FileMode.OpenOrCreate);
            stream.Result.CopyTo(fs);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("InvalidOperationException: {0}", ex.HelpLink);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("HttpRequestException: {0}", ex.HelpLink);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("Task was aborted. {0}", ex.HelpLink);
        }
        return false;
    }

    static bool UnzipFile(string compressedFile, string targetDirectory, bool overwrite = false)
    {
        if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
        try
        {
            ZipFile.ExtractToDirectory(compressedFile, targetDirectory, overwrite);
            return true;
        }
        catch
        {
            Console.WriteLine("Unhandled Exception! Unzip stopped.");
        }
        return false;
    }

    static void OpenPort(Protocol protocol, int port, string name)
    {
        try
        {
            Task.Run(async () =>
            {
                var discoverer = new NatDiscoverer();

                var device = await discoverer.DiscoverDeviceAsync();

                await device.CreatePortMapAsync(new Mapping(protocol, port, port, name));

                await Task.Delay(1000);
            }).Wait();

            Console.WriteLine("Done, depending on your router, there should now be a open port targeting the port {0} named '{1}' using protocol {2}", port, name, protocol switch { Protocol.Tcp => "Tcp", Protocol.Udp => "Udp", _ => "null", });
        }
        catch (NatDeviceNotFoundException ex)
        {
            Console.WriteLine("Error: Unable to automatically open the desired ports. {0}", ex.HelpLink);
        }
        catch (MappingException ex)
        {
            switch (ex.ErrorCode)
            {
                case 718:
                    Console.WriteLine("Error: The ports are already in use. {0}", ex.HelpLink);
                    break;

                case 728:
                    Console.WriteLine("Error: Your routers mapping table is full. {0}", ex.HelpLink);
                    break;
            }
        }
        Thread.Sleep(1000);
    }

    static void StartServer(ServerObject server)
    {
        ConsoleExtension.Propmpt("Do you wish to attempt to automatically open the port using UPnP? y/n", out string choice, Validators.YesNoValidator);
        if (Converters.YesNoConvert(choice))
        {
            OpenPort(Protocol.Udp, server.Server.Port, "Rust Dedicated Server");
        }
        ConsoleExtension.Propmpt("Do you want to attempt to automatically open a port for Rust+? y/n", out string rpchoice, Validators.YesNoValidator);
        if (Converters.YesNoConvert(rpchoice))
        {
            var port = (server.Server.Port > server.Rcon.Port) ? server.Server.Port : server.Rcon.Port;
            OpenPort(Protocol.Tcp, port + 67, "Rust+");
        }
        Console.Clear();
        Console.WriteLine($"Starting {server.Server.Identity}");
        Thread.Sleep(300);
        Console.Clear();

        Console.Title = $"Rust Server Running: {server.Server.Identity}";

        bool fistStart = true;

        if (settings.AutoRestart)
        {
            Console.WriteLine("Warning, using autorestart can lead to the rust dedicated process to not exit if you close this app. When you close, make sure you check taskmanager.");
            Thread.Sleep(5000);
            Console.Clear();
        }

        while (settings.AutoRestart || fistStart)
        {
            // Start rust server:
            Process process = new();
            process.StartInfo.FileName = settings.RustDedicatedPath;
            process.StartInfo.Arguments = server.CreateProcessArguments();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(settings.RustDedicatedPath);

            process.Start();
            process.WaitForExit();

            fistStart = false;

            if (settings.AutoRestart)
            {
                Console.WriteLine("Restarting the server in 10 seconds");
                Thread.Sleep(10000);
            }
        }

        Console.WriteLine("Server Closed. Thank you for using Rust Manager Console");
        Thread.Sleep(1000);
    }
}