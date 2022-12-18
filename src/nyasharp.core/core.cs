using System.Runtime.CompilerServices;
using System.Text;
using nyasharp.AST;
using nyasharp.Interpreter;
using Environment = nyasharp.Interpreter.Environment;

namespace nyasharp
{
    public class core
    {
        // Incase we get an error, don't execute the code
        private static Interpreter.Interpreter _interpreter = new();
        public static Events.PrintWorker PrintWorker = new();
        public static Events.ErrorWorker ErrorWorker = new();
        
        public static bool HadParseError = false;
        public static void Run(string source)
        {
            // Tokenize
            var tokens = Tokenize(source);

            // Parse
            var statements= Parse(tokens);

            if (HadParseError) return;

                // Resolve
            Resolve(statements);

            _interpreter.Interpret(statements);
        }

        public static List<Token> Tokenize(string source)
        {
            Scanner.Scanner scanner = new Scanner.Scanner(source);
            return scanner.ScanTokens();
        }

        public static List<Stmt?> Parse(List<Token> tokens)
        {
            var parser = new Parser.Parser(tokens);
            return parser.Parse();
        }

        public static void Resolve(List<Stmt?> statements)
        {
            var resolver = new Resolver.Resolver(_interpreter);
            resolver.Resolve(statements);
        }

        private static void Report(int line, string where, string message)
        {
            ErrorWorker.Invoke("[line " + line + "] Error" + where + ": " + message);
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
            tmp.Append("\n[line " + error.token.line + "] " + error.Message);
            var err = new StringWriter(tmp);
            ErrorWorker.Invoke(err.ToString());
        }
    }
}