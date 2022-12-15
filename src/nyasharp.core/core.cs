using System.Text;
using nyasharp.AST;
using nyasharp.Interpreter;

namespace nyasharp
{
    public class Result
    {
        public object? Value { get; }
        public List<string> Errors { get; }
        public Result(object? value, List<string> errors)
        {
            this.Value = value;
            this.Errors = errors;
        }
    }
    public class core
    {
        // Incase we get an error, don't execute the code
        private static Interpreter.Interpreter _interpreter = new Interpreter.Interpreter();
        // List for errors
        private static List<string> _errors = new List<string>();
        public static Result Run(string source)
        {
            _errors.Clear();
            // Tokenize
            Scanner.Scanner scanner = new Scanner.Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            
            // Parse
            var parser = new Parser.Parser(tokens);
            List<Stmt?> statements = parser.Parse();

            _interpreter.Interpret(statements);

            return new Result(true, _errors);
        }

        private static void Report(int line, string where, string message)
        {
            _errors.Add("[line " + line + "] Error" + where + ": " + message);
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
            _errors.Add(err.ToString());
        }
    }
}