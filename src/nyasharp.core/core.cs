using System.Text;
using nyasharp.AST;
using nyasharp.Interpreter;
using Environment = System.Environment;

namespace nyasharp
{
    public class core
    {
        // Incase we get an error, don't execute the code
        private static Interpreter.Interpreter _interpreter = new Interpreter.Interpreter();
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
        public static void Run(string source)
        {
            if (_hadError)
            {
                Console.WriteLine();
                Environment.Exit(65);
            }

            // Tokenize
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            
            // Parse
            var parser = new Parser.Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (_hadRuntimeError)
            {
                Console.WriteLine();
                Environment.Exit(70);
            }
            
            _interpreter.interpret(statements);
            
            // Console.WriteLine(new Printer().Print(expressions));
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
            tmp.Append("\n[line " + error.token.line + "] " + error.Message);
            var err = new StringWriter(tmp);
            Console.SetError(err);
            Console.Write(err);
            _hadRuntimeError = true;
        }
    }
}