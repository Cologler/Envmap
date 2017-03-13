using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envmap
{
    public class CommandBuilder
    {
        public static StringComparer Comparer => StringComparer.OrdinalIgnoreCase;

        private static readonly Dictionary<string, Action<EnvironmentVariableEditor, string>> Actions;
        private static readonly Dictionary<string, EnvironmentVariableTarget> Targets;
        private static readonly Dictionary<string,
            Func<string, EnvironmentVariableTarget, EnvironmentVariableEditor>> Editors;

        static CommandBuilder()
        {
            Targets = new Dictionary<string, EnvironmentVariableTarget>(Comparer)
            {
                ["user"] = EnvironmentVariableTarget.User,
                ["machine"] = EnvironmentVariableTarget.Machine
            };

            Editors = new Dictionary<string, Func<string, EnvironmentVariableTarget, EnvironmentVariableEditor>>()
            {
                ["path"] = (n, t) => new PathEnvironmentVariableEditor(n, t)
            };

            Actions = new Dictionary<string, Action<EnvironmentVariableEditor, string>>(Comparer)
            {
                ["add"] = (e, v) => e.Add(v),
                ["set"] = (e, v) => e.Set(v),
                ["map"] = (e, v) => e.Map(v),
                ["remove"] = (e, v) => e.Remove(v),
            };
        }

        private string command;
        private string value;
        private string name;
        private EnvironmentVariableTarget target = EnvironmentVariableTarget.User;

        public void ReadFromArgs(string[] args)
        {
            if (args.Length < 4) throw new ArgumentException($"missing arguments. (count of arguments must >= 4)");

            var c = args[0];
            if (!Actions.ContainsKey(c)) throw new ArgumentException($"unknown command {c}");
            this.command = c;

            this.value = args[1];

            if (!Comparer.Equals(args[2], "--to")) throw new ArgumentException("3rd arg must be \"--to\"");

            this.name = args[3];

            if (args.Length == 4) return;

            for (var i = 4; i < args.Length; i++)
            {
                var name = args[i];
                if (!name.StartsWith("--")) throw new ArgumentException($"please add \"--\" bdfore args name: <{name}>");
                name = name.Substring(2);

                if (Comparer.Equals(name, "target"))
                {
                    if (args.Length > i + 1)
                    {
                        i++;
                        if (Targets.TryGetValue(args[i], out var val))
                        {
                            this.target = val;
                        }
                        else throw new ArgumentException($"unknown args value: <{args[i]}>");
                    }
                    else throw new ArgumentException($"arg <--{name}> missing value.");
                }
                else throw new ArgumentException($"unknown args name: <--{name}>");
            }
        }

        public Action Build()
        {
            var cmd = this.command;
            var val = this.value;
            var nam = this.name;
            var tar = this.target;
            return () =>
            {
                var e = (Editors.TryGetValue(nam, out var k) ? k : (n, t) => new EnvironmentVariableEditor(n, t))(nam, tar);
                e.LoadFromTarget();
                Actions[cmd](e, e.CheckArgument(val));
                e.SaveToTarget();
            };
        }

        public static void PrintUsage()
        {
            Console.WriteLine("usage:");
            var cmdKeys = string.Join("|", Actions.Keys);
            var targetKeys = string.Join("|", Targets.Keys);
            Console.WriteLine($"   envmap ({cmdKeys}) VALUE --to NAME [--target ({targetKeys})]");
        }
    }
}
