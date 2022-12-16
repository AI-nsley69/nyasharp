
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
            AttachEvents();
            var bytes = File.ReadAllBytes(Path.GetFullPath(path));
            Run(Encoding.UTF8.GetString(bytes));
            DetachEvents();
        }

        private static void RunPrompt()
        {
            AttachEvents();
            for (;;)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                Run(line);
            }
            DetachEvents();
        }

        private static void Run(string code)
        {
            core.Run(code);
        }

        private static void AttachEvents()
        {
            core.PrintWorker.OnPrint += WorkerPrint;
            core.ErrorWorker.OnError += WorkerError;
        }

        private static void DetachEvents()
        {
            core.PrintWorker.OnPrint -= WorkerPrint;
            core.ErrorWorker.OnError -= WorkerError;
        }

        private static void WorkerPrint(string str)
        {
            Console.WriteLine(str);
        }

        private static void WorkerError(string str)
        {
            var tmp = new StringBuilder(str);
            var err = new StringWriter(tmp);
            Console.SetError(err);
            Console.WriteLine(err);
        }
    }
}