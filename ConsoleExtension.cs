namespace RustManagerConsole;

static class ConsoleExtension
{
    public static void CreateMenu(string Title, string[] Options, out int Selection, bool ClearConsole = true)
    {
        int maxInt = Options.Length;
        int selectedInt;
        while (true)
        {
            if (ClearConsole) Console.Clear();
            Console.WriteLine(Title);
            for (int i = 0; i < maxInt; i++)
            {
                Console.WriteLine($"{i + 1}. {Options[i]}");
            }
            string? input = Console.ReadLine();
            if (input != null)
            {
                if (int.TryParse(input, out selectedInt))
                {
                    if (selectedInt - 1 < maxInt && selectedInt - 1 >= 0)
                    {
                        break;
                    }
                }
            }
        }
        Selection = selectedInt - 1;
    }

    public static void Propmpt(string Title, out string returnValue, int min = 1, bool ClearConsole = true)
    {
        string? input;
        while (true)
        {
            if (ClearConsole) Console.Clear();
            Console.WriteLine(Title);
            input = Console.ReadLine();
            if (input != null)
            {
                if (input.Length >= min)
                {
                    break;
                }
            }
        }
        returnValue = input;
    }

    public static void Propmpt(string Title, out string returnValue, Func<string, bool> Validator, int min = 1, bool ClearConsole = true)
    {
        string? input;
        while (true)
        {
            if (ClearConsole) Console.Clear();
            Console.WriteLine(Title);
            input = Console.ReadLine();
            if (input != null)
            {
                if (input.Length >= min)
                {
                    if (Validator(input))
                    {
                        break;
                    }
                }
            }
        }
        returnValue = input;
    }
}
