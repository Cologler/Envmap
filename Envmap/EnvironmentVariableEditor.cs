using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envmap
{
    public class EnvironmentVariableEditor
    {
        protected readonly List<string> changeLog = new List<string>();
        protected readonly List<string> paths = new List<string>();

        public EnvironmentVariableEditor(string name, EnvironmentVariableTarget target)
        {
            this.Name = name;
            this.Target = target;
        }

        public string Name { get; }

        public EnvironmentVariableTarget Target { get; }

        public void LoadFromTarget()
        {
            var text = Environment.GetEnvironmentVariable(this.Name, this.Target);
            if (text != null) this.LoadFrom(text);
            else throw new EnvmapRuntimeException($"unknown env var name: <{this.Name}>");
        }

        public void SaveToTarget()
        {
            if (this.changeLog.Count == 0) return; // nothing changed.

            var result = this.ToString();
            Environment.SetEnvironmentVariable(this.Name, result, this.Target);

            this.changeLog.ForEach(Console.WriteLine);
        }

        protected virtual void LoadFrom(string value)
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

            this.paths.Clear();
            this.paths.AddRange(p
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(z => z != string.Empty));
        }

        protected virtual void OnAdded(string value)
        {
            this.changeLog.Add($"added value <{value}>.");
        }

        protected virtual void OnRemoved(string value)
        {
            this.changeLog.Add($"removed value <{value}>.");
        }

        public virtual string CheckArgument(string value) => value;

        public virtual void Add(string value)
        {
            var exists = this.paths.FirstOrDefault(z => CommandBuilder.Comparer.Equals(z, value));
            if (exists != null) throw new EnvmapRuntimeException($"exists value: <{exists}>");
            this.OnAdded(value);
            this.paths.Add(value);
        }

        public virtual void Remove(string value)
        {
            var exists = this.paths.FirstOrDefault(z => CommandBuilder.Comparer.Equals(z, value));
            if (exists == null) throw new EnvmapRuntimeException($"value not exists: <{value}>");
            this.OnRemoved(exists);
            this.paths.Remove(exists);
        }

        public virtual void Map(string value)
        {
            throw new EnvmapRuntimeException($"{this.Name} environment variable cannot map value.");
        }

        public virtual void Set(string value)
        {
            this.paths.ForEach(this.OnRemoved);
            this.paths.Clear();

            this.OnAdded(value);
            this.paths.Add(value);            
        }

        public override string ToString()
        {
            var p = this.paths.Where(z => z != string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            p.Sort();
#if DEBUG
            p.ForEach(Console.WriteLine);
#endif
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
    }
}
