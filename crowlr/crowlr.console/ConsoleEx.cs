using System;

namespace crowlr.console
{
    public static class ConsoleEx
    {
        public static void Warning(string text)
        {
            Write(text, ConsoleColor.Yellow);
        }

        public static void WarningLine(string text)
        {
            WriteLine(text, ConsoleColor.Yellow);
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            ChangeColor(color, () => Console.WriteLine(text));
        }

        public static void Write(string text, ConsoleColor color)
        {
            ChangeColor(color, () => Console.Write(text));
        }

        public static void ChangeColor(ConsoleColor color, Action action)
        {
            var currentColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            action();

            Console.ForegroundColor = currentColor;
        }
    }
}
