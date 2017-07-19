using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jasily.Frameworks.Cli.Attributes;
using Jasily.Frameworks.Cli.IO;

namespace Envmap
{
    [CommandClass(IsNotResult = true)]
    class EnvironmentVariableList : IEnvironmentVariableValue
    {
        private static StringComparer Comparer => StringComparer.OrdinalIgnoreCase;
        private readonly string _name;
        private readonly EnvironmentVariableTarget _target;
        private readonly string _originValue;
        protected readonly List<string> Paths = new List<string>();

        public EnvironmentVariableList(string name, EnvironmentVariableTarget target, string originValue)
        {
            this._name = name;
            this._target = target;
            this._originValue = originValue;
            this.Paths.AddRange(Parse(originValue));
        }

        private static IEnumerable<string> Parse(string value)
        {
            var sb = new StringBuilder();
            var p = new List<string>();
            var scope = false;

            void EndOfLine()
            {
                if (sb.Length > 0)
                {
                    p.Add(sb.ToString().Trim());
                    sb.Clear();
                }
            }

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '"':
                        scope = !scope;
                        break;

                    case ';':
                        if (scope) goto default;
                        EndOfLine();
                        break;


                    default:
                        sb.Append(ch);
                        break;
                }
            }

            EndOfLine();
            return p.Distinct(StringComparer.OrdinalIgnoreCase).Where(z => z != string.Empty);
        }

        private static string ToStringValue(IEnumerable<string> values)
        {
            var p = values.Where(z => !string.IsNullOrWhiteSpace(z)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var sb = new StringBuilder();
            foreach (var item in p)
            {
                var line = item;
                if (line.Contains(";"))
                {
                    sb.Append("\"");
                    sb.Append(line);
                    sb.Append("\"");
                }
                else
                {
                    sb.Append(line);
                }
                sb.Append(';');
            }
            sb.Remove(sb.Length - 1, 1); // remove last ';'
            return sb.ToString();
        }

        [CommandMethod]
        public void List(IOutputer outputer)
        {
            outputer.WriteLine(OutputLevel.Normal, this.ToString());
        }

        [CommandMethod]
        public void Sort(IOutputer outputer)
        {
            this.Paths.Sort();
            this.Save();
            outputer.WriteLine(OutputLevel.Normal, $"Sorted {this._target} envvar <{this._name}>.");
        }

        public virtual void Add(string value, IOutputer outputer)
        {
            var exists = this.Paths.FirstOrDefault(z => Comparer.Equals(z, value));
            if (exists != null) throw new InvalidOperationException($"exists value: <{exists}>");
            this.Paths.Add(value);
            this.Save();
            outputer.WriteLine(OutputLevel.Normal, $"Added <{value}> into {this._target} envvar <{this._name}>.");
        }

        public virtual void Remove(string value, IOutputer outputer)
        {
            var exists = this.Paths.FirstOrDefault(z => Comparer.Equals(z, value));
            if (exists == null) throw new InvalidOperationException($"value not exists: <{value}>");
            this.Paths.Remove(exists);
            this.Save();
            outputer.WriteLine(OutputLevel.Normal, $"Removed <{value}> from {this._target} envvar <{this._name}>.");
        }

        protected void Save()
        {
            var val = ToStringValue(this.Paths);
            Environment.SetEnvironmentVariable(this._name, val, this._target);
        }

        public override string ToString()
        {
            return string.Join("\r\n", this.Paths);
        }
    }
}