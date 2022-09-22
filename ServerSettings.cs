namespace RustManagerConsole;

struct ServerSettings
{
    public ServerSettings() { }

    public string Identity { get; set; } = "Server 1";
    public string IP { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 28015;
    public int TickRate { get; set; } = 30;
    public string HostName { get; set; } = "My Server";
    public string Description { get; set; } = "My awesome server";
    public int MaxPlayers { get; set; } = 10;
    public int SaveInterval { get; set; } = 600;
    public int WorldSize { get; set; } = 3000;
    public int Seed { get; set; } = 0;

    public string CreateServerArguments()
    {
        return $"+server.ip {IP} +server.port {Port} +server.hostname \"{HostName}\" +server.description \"{Description}\" +server.maxplayers {MaxPlayers} +server.worldsize {WorldSize} +server.seed {Seed} +server.identity \"{Identity}\" +server.tickrate {TickRate} +server.saveinterval {SaveInterval}";
    }
}
