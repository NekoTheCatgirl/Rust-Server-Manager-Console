namespace RustManagerConsole;

struct ServerObject
{
    public ServerObject(ServerSettings server, RconSettings rcon)
    {
        Server = server;
        Rcon = rcon;
    }

    public ServerSettings Server { get; set; }
    public RconSettings Rcon { get; set; }
    public string LogFile { get; set; } = "gamelog.txt";
    public bool SilentCrashes { get; set; } = false;

    public string CreateProcessArguments()
    {
        return $"-batchmode {Server.CreateServerArguments()} {Rcon.CreateRconArguments()} -logfile \"{LogFile}\"{(SilentCrashes ? " -silent-crashes" : "")}";
    }

    public

    static ServerObject CreateNewServer()
    {

        // Server settings
        Console.Clear();
        Console.WriteLine("Server Side Settings:");
        Thread.Sleep(1000);

        ConsoleExtension.Propmpt("Server Identity", out string ServerIdentity, 1);
        ConsoleExtension.Propmpt("Server Port", out string ServerPortStr, Validators.IsIntValidator, 2);
        ConsoleExtension.Propmpt("Server Hostname", out string HostName, 3);
        ConsoleExtension.Propmpt("Server Description", out string Description, 0);
        ConsoleExtension.Propmpt("Server Max Players", out string MaxPlayersStr, Validators.IsIntValidator);
        ConsoleExtension.Propmpt("Server World Size (3000-6000)", out string WorldSizeStr, input => { if (Validators.IntRangeCheck(input, 3000, 6000)) return true; return false; }, 4);
        ConsoleExtension.Propmpt("Generate random seed? y/n", out string RNGSeedOption, Validators.YesNoValidator);
        int Seed;
        if (Converters.YesNoConvert(RNGSeedOption))
        {
            Seed = new Random().Next(1, 100000);
        }
        else
        {
            ConsoleExtension.Propmpt("Server Seed", out string SeedStr, Validators.IsIntValidator);
            Seed = int.Parse(SeedStr);
        }

        // Apply Server Settings
        ServerSettings ServerCfg = new()
        {
            Identity = ServerIdentity,
            Port = int.Parse(ServerPortStr),
            HostName = HostName,
            Description = Description,
            MaxPlayers = int.Parse(MaxPlayersStr),
            WorldSize = int.Parse(WorldSizeStr),
            Seed = Seed
        };

        Console.Clear();
        Console.WriteLine("Server Side Settings Complete");
        Thread.Sleep(300);

        // Rcon settings
        Console.Clear();
        Console.WriteLine("Rcon Settings:");
        Thread.Sleep(1000);

        ConsoleExtension.Propmpt("Rcon Port", out string RconPortStr, Validators.IsIntValidator, 2);
        ConsoleExtension.Propmpt("Rcon Web1? y/n", out string RconWeb1Option, Validators.YesNoValidator);
        ConsoleExtension.Propmpt("Rcon Password", out string RconPassword);

        // Apply Rcon Settings
        RconSettings RconCfg = new()
        {
            Port = int.Parse(RconPortStr),
            Web1 = Converters.YesNoConvert(RconWeb1Option),
            Password = RconPassword
        };

        Console.Clear();
        Console.WriteLine("Rcon Settings Complete");
        Thread.Sleep(300);

        return new ServerObject(ServerCfg, RconCfg);
    }
}
