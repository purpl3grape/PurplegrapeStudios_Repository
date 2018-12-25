using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Obfuscator.Attribute
{
    /// <summary>
    /// Add this to an Class, Field, Method, whatever and it will not get renamed!
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DoNotRenameAttribute : System.Attribute
    {
    }
}
