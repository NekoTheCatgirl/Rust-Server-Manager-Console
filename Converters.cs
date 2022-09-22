namespace RustManagerConsole;

public static class Converters
{
    public static bool YesNoConvert(string input)
    {
        string[] yes = new[] { "y", "yes", "true" };
        if (yes.Contains(input.ToLower()))
            return true;
        return false;
    }
}
