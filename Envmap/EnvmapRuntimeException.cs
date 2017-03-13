using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envmap
{
    public class EnvmapRuntimeException : Exception
    {
        public EnvmapRuntimeException(string message) : base(message) { }
    }
}
