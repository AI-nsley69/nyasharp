
using System.Text;

namespace nyasharp.cli
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nyasharp.core [script]\n");
                return 64;
            }

            if (args.Length == 1)
            {
                if (!args[0].EndsWith(".nya"))
                {
                    Console.WriteLine(
                        "UwU! Sow sowwy. It wooks wike ywou mwade a fucky wucky. This fiwe is not .nya :c");
                    return 0;
                }
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
            core.Run(Encoding.UTF8.GetString(bytes));
        }

        private static void RunPrompt()
        {
            for (;;)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                Result? result = null; 
                try
                {
                    result = core.Run(line);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }

                if (result == null) continue;
                if (result.Value is string s && s.Length > 0) Console.WriteLine(s);
                if (result.Errors.Count != 0)
                {
                    result.Errors.ForEach(Console.WriteLine);
                }
            }
        }
    }
}