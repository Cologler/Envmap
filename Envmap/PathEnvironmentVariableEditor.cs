using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envmap
{
    public class PathEnvironmentVariableEditor : EnvironmentVariableEditor
    {
        public PathEnvironmentVariableEditor(string name, EnvironmentVariableTarget target)
            : base(name, target)
        {
        }

        public override string CheckArgument(string value)
        {
            if (!Directory.Exists(value)) throw new EnvmapRuntimeException($"invalid directory: <\"{value}\">.");
            return Path.GetFullPath(value);
        }

        protected override void OnAdded(string value)
        {
            this.changeLog.Add($"added directory <{value}>.");
        }

        protected override void OnRemoved(string value)
        {
            this.changeLog.Add($"removed directory <{value}>.");
        }

        public override void Map(string value)
        {
            var lc = value[value.Length - 1];
            if (lc == Path.DirectorySeparatorChar || lc == Path.AltDirectorySeparatorChar)
            {
                value = value.Substring(0, value.Length - 1);
            }
            var p = value;
            var sp = value + Path.DirectorySeparatorChar;
            var set = new HashSet<string>(CommandBuilder.Comparer);
            var add = new List<string>();
            this.paths.RemoveAll(z =>
                {
                    if (z.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                        z.StartsWith(sp, StringComparison.OrdinalIgnoreCase))
                    {
                        set.Add(z);
                        return true;
                    }
                    return false;
                });

            foreach (var item in Directory.GetDirectories(value))
            {
                if (!set.Remove(item)) add.Add(item);
                this.paths.Add(item);
            }
            set.ToList().ForEach(this.OnRemoved);
            add.ForEach(this.OnAdded);
        }

        public override void Set(string value)
        {
            throw new EnvmapRuntimeException($"{this.Name} environment variable cannot set value.");
        }
    }
}
