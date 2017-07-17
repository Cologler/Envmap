using System;
using System.Threading.Tasks;
using Jasily.Frameworks.Cli;
using Jasily.Frameworks.Cli.Attributes;
using Jasily.Frameworks.Cli.Exceptions;

namespace Envmap
{
    [CommandClass()]
    class Program
    {
        static void Main(string[] args)
        {
            new EngineBuilder().InstallConsoleOutput().Build().Fire(new Program()).Execute(args);
        }

        [CommandMethod(Names = new[] {"Add-Var"}, IgnoreDeclaringName = true)]
        public string AddVar(string envVar, string value,
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            var v = Environment.GetEnvironmentVariable(envVar, target);
            if (v != null) throw new InvalidOperationException($"envvar {envVar} is exists.");
            Environment.SetEnvironmentVariable(envVar, value, target);
            return "operation completed.";
        }

        public IEnvironmentVariableValue Var(string envVar, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            var value = Environment.GetEnvironmentVariable(envVar, target);
            if (value == null) throw new InvalidOperationException($"envvar {envVar} is NOT exists.");
            return new EnvironmentVariableDirectoryList(envVar, target, value);
        }
    }
}
