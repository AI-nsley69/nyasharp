using System.Text;
using nyasharp.AST;
using nyasharp.Interpreter;
using Environment = System.Environment;

namespace nyasharp
{
    internal class Program
    {
        // Incase we get an error, don't execute the code
        private static Interpreter.Interpreter _interpreter = new Interpreter.Interpreter();
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
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
        }

        private static void RunPrompt()
        {
            for (;;)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                Run(line);
                _hadError = false;
            }
        }

        private static void Run(string source)
        {
            if (_hadError) Environment.Exit(65);

            // Tokenize
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            // Parse
            var parser = new Parser.Parser(tokens);
            List<Stmt> statements = parser.Parse();
            
            if (_hadRuntimeError) Environment.Exit(70);
            
            _interpreter.interpret(statements);
            
            // Console.WriteLine(new Printer().Print(expressions));
            
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

        public static void RuntimeError(RuntimeError error)
        {
            var tmp = new StringBuilder();
            tmp.Append(error.Message + "\n[line " + error.token.line + "]");
            var err = new StringWriter(tmp);
            Console.SetError(err);
            _hadRuntimeError = true;
        }
    }
}