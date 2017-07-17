using System;
using Jasily.Frameworks.Cli.Attributes;
using Jasily.Frameworks.Cli.IO;

namespace Envmap
{
    [CommandClass(IsNotResult = true)]
    class EnvironmentVariableString : IEnvironmentVariableValue
    {
        private readonly string _name;
        private readonly EnvironmentVariableTarget _target;
        private readonly string _originValue;

        public EnvironmentVariableString(string name, EnvironmentVariableTarget target, string originValue)
        {
            this._name = name;
            this._target = target;
            this._originValue = originValue;
        }

        public void Show(IOutputer outputer)
        {
            outputer.WriteLine(OutputLevel.Normal, this._originValue);
        }

        public void Set(string value)
        {
            Environment.SetEnvironmentVariable(this._name, value, this._target);
        }
    }
}