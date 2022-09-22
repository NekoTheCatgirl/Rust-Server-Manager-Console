namespace RustManagerConsole;

struct RconSettings
{
    public RconSettings() { }

    public string Password { get; set; } = "letmein";
    public bool Web1 { get; set; } = true;
    public string IP { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 28016;

    public string CreateRconArguments()
    {
        return $"+rcon.ip {IP} +rcon.port {Port} +rcon.password \"{Password}\"{(Web1 ? " +rcon.web1" : "")}";
    }
}
