using System;
using System.Collections.Generic;
using System.IO;
using Jasily.Frameworks.Cli.Attributes;
using Jasily.Frameworks.Cli.IO;

namespace Envmap
{
    class EnvironmentVariableDirectoryList : EnvironmentVariableList
    {
        public EnvironmentVariableDirectoryList(string name, EnvironmentVariableTarget target, string originValue) 
            : base(name, target, originValue)
        {
        }

        public override void Add(string dirPath, IOutputer outputer)
        {
            if (!Directory.Exists(dirPath)) throw new InvalidOperationException($"invalid directory: <\"{dirPath}\">.");
            base.Add(Path.GetFullPath(dirPath), outputer);
        }

        public override void Remove(string dirPath, IOutputer outputer)
        {
            base.Remove(Path.GetFullPath(dirPath), outputer);
        }

        public void Map(string dirPath, IOutputer outputer)
        {
            if (!Directory.Exists(dirPath)) throw new InvalidOperationException($"invalid directory: <\"{dirPath}\">.");
            dirPath = Path.GetFullPath(dirPath);

            var lastChar = dirPath[dirPath.Length - 1];
            if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar)
            {
                dirPath = dirPath.Substring(0, dirPath.Length - 1);
            }

            var dirPath2 = dirPath + Path.DirectorySeparatorChar;
            var removed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var added = new List<string>();
            this.Paths.RemoveAll(z =>
            {
                if (z.Equals(dirPath, StringComparison.OrdinalIgnoreCase) ||
                    z.StartsWith(dirPath2, StringComparison.OrdinalIgnoreCase))
                {
                    removed.Add(z);
                    return true;
                }
                return false;
            });

            foreach (var item in Directory.GetDirectories(dirPath))
            {
                if (!removed.Remove(item)) added.Add(item);
                this.Paths.Add(item);
            }

            outputer.WriteLine(OutputLevel.Normal, $"Added ({added.Count}):");
            foreach (var item in added)
            {
                outputer.WriteLine(OutputLevel.Normal, $"   {item}");
            }
            outputer.WriteLine(OutputLevel.Normal, $"Removed ({removed.Count}):");
            foreach (var item in removed)
            {
                outputer.WriteLine(OutputLevel.Normal, $"   {item}");
            }

            this.Save();
        }
    }
}