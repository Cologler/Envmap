using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envmap
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new CommandBuilder();
            try
            {
                builder.ReadFromArgs(args);
            }
            catch (ArgumentException error)
            {
                Console.WriteLine(error.Message);
                CommandBuilder.PrintUsage();
                return;
            }
            try
            {
                builder.Build().Invoke();
            }
            catch (EnvmapRuntimeException error)
            {
                Console.WriteLine("<Error>: " + error.Message);
            }
        }

        static void Map(string path)
        {
            var editor = new PathEnvironmentVariableEditor("PATH", EnvironmentVariableTarget.User);
            editor.LoadFromTarget();
            editor.Map(path);
            Console.WriteLine(editor.ToString());
        }
    }
}
