using System.Linq;
using System.Xml.Linq;

namespace Libclang.Core.Parser
{
    public static class XElementExtensions
    {
        public static string GetAttributeValue(this XElement xElement, XName name)
        {
            var attribute = xElement.Attribute(name);

            if (attribute == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(attribute.Value))
            {
                return null;
            }

            return attribute.Value;
        }

        public static bool HasTrueAttribute(this XElement xElement, string name)
        {
            var value = xElement.GetAttributeValue(name);

            if (value == null)
            {
                return false;
            }

            return value == "true";
        }
    }
}
