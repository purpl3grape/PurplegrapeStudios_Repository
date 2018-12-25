using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Obfuscator.Attribute
{
    /// <summary>
    /// Add this to an Class, and the whole class with content will not get obfuscated! But still its Method Body.
    /// To not obfuscate Method Body too, add an additional Attribute: DoNotObfuscateMethodBodyAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotObfuscateClassAttribute : System.Attribute
    {
    }
}
