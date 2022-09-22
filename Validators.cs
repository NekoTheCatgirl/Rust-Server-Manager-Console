namespace RustManagerConsole;

public static class Validators
{
    public static bool IsIntValidator(string input)
    {
        if (int.TryParse(input, out _))
            return true;
        return false;
    }

    public static bool IntRangeCheck(string input, int min, int max)
    {
        if (IsIntValidator(input))
            if (int.Parse(input) >= min && int.Parse(input) <= max)
                return true;
        return false;
    }

    public static bool YesNoValidator(string input)
    {
        string[] valids = new[] { "y", "yes", "true", "n", "no", "false" };
        if (valids.Contains(input.ToLower()))
            return true;
        return false;
    }
}
