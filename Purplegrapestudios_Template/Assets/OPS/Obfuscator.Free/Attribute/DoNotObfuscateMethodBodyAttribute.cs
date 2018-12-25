using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Obfuscator.Attribute
{
    /// <summary>
    /// Add this to an Class to skip obfuscation of all Method Bodies, or to an specific Method to skip its Method Body.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DoNotObfuscateMethodBodyAttribute : System.Attribute
    {
    }
}
