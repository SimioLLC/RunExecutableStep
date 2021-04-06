using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestRunArguments
{
    class Program
    {
        static void Main(string[] args)
        {
            string outpath = "";
            try
            {

                // Let's put the output file in the same folder as where this exe is running
                string executingAssembly = Assembly.GetExecutingAssembly().Location;
                outpath = Path.Combine(Path.GetDirectoryName(executingAssembly), "TestRunArguments.txt");
                Console.WriteLine($"OutputPath={outpath}");

                StringBuilder sbLog = new StringBuilder();

                sbLog.AppendLine("");
                sbLog.AppendLine($"{DateTime.Now:HH:mm:ss.ff}: CommandLine=[{Environment.CommandLine}]");

                sbLog.AppendLine($"    There are {args.Length} arguments");
                int ii = 0;
                foreach (string arg in args)
                {
                    sbLog.AppendLine($"    Arg[{ii++}]=[{arg}]");
                }

                File.AppendAllText(outpath, sbLog.ToString());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Oops. Outpath={outpath} Err={ex.Message}");
            }

        }
    }
}
