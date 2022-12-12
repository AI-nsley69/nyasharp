using nyasharp.AST;

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
            for (;;)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                Run(line);
            }
        }

        private static void Run(string source)
        {
            // Tokenize
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            // Parse
            var parser = new Parser.Parser(tokens);
            Expr expressions = parser.Parse();

            if (_hadError) return;
            
            Console.WriteLine(new Printer().Print(expressions));
            /* Print tokens for now
            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
            */
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            _hadError = true;
        }
        
        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }
    }
}