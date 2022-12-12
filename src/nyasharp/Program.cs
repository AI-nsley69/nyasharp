namespace nyasharp
{
    internal class Program
    {
        // Incase we get an error, don't execute the code
        private static bool _hadError = false;
        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nyasharp [script]");
                return 64;
            }

            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            return 0;
        }

        private static void RunFile(string path)
        {
            var bytes = File.ReadAllBytes(Path.GetFullPath(path));
            Run(System.Text.Encoding.Default.GetString(bytes));
            if (_hadError) Environment.Exit(65);
        }

        private static void RunPrompt()
        {
            return;
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            
            // Print tokens for now
            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            _hadError = true;
        }
    }
}