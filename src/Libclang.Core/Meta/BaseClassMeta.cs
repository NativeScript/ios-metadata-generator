using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public abstract class BaseClassMeta : Meta
    {
        public HashSet<MethodMeta> Methods { get; set; }

        public HashSet<PropertyMeta> Properties { get; set; }

        public IEnumerable<string> ImplementedProtocolsJSNames { get; set; }

        public IEnumerable<MethodMeta> StaticMethods
        {
            get { return this.Methods.Where(m => m.IsStatic); }
        }

        public IEnumerable<MethodMeta> InstanceMethods
        {
            get { return this.Methods.Where(m => !m.IsStatic); }
        }

        public IEnumerable<ProtocolMeta> ImplementedProtocols
        {
            get { return this.ImplementedProtocolsJSNames.Select(n => (ProtocolMeta) this.Container[n]); }
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            return this.Serialize(this.InstanceMethods, this.StaticMethods, this.Properties,
                this.ImplementedProtocolsJSNames);
        }

        protected virtual BinaryMetaStructure Serialize(
            IEnumerable<MethodMeta> instanceMethods,
            IEnumerable<MethodMeta> staticMethods,
            IEnumerable<PropertyMeta> properties,
            IEnumerable<string> protocolsNames)
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();
            StringAsciiComparer comparer = new StringAsciiComparer();

            List<MethodMeta> instanceMethodsList = instanceMethods.OrderBy(m => m.JSName, comparer).ToList();
            int firstInitializerIndex = -1;
            for (int i = 0; i < instanceMethodsList.Count; i++)
            {
                if (instanceMethodsList[i].Name.StartsWith("init"))
                {
                    firstInitializerIndex = i;
                    break;
                }
            }

            List<object> instanceMethodsStructuresList = instanceMethodsList.Select(m => (object) new Pointer(m.GetBinaryStructure())).ToList();
            instanceMethodsStructuresList.Insert(0, new ArrayCount((uint) instanceMethodsStructuresList.Count));
            List<object> staticMethodsList = staticMethods.OrderBy(m => m.JSName, comparer).Select(m => (object) new Pointer(m.GetBinaryStructure())).ToList();
            staticMethodsList.Insert(0, new ArrayCount((uint) staticMethodsList.Count));
            List<object> propertiesList = properties.OrderBy(p => p.JSName, comparer).Select(m => (object) new Pointer(m.GetBinaryStructure())).ToList();
            propertiesList.Insert(0, new ArrayCount((uint) propertiesList.Count));
            List<object> protocolsList = protocolsNames.OrderBy(p => p, comparer).Distinct().Select(p => (object) new Pointer(p)).ToList();
            protocolsList.Insert(0, new ArrayCount((uint) protocolsList.Count));

            List<object> membersLists = new List<object>()
            {
                new Pointer(instanceMethodsStructuresList),
                new Pointer(staticMethodsList),
                new Pointer(propertiesList),
                new Pointer(protocolsList),
                (short) firstInitializerIndex
            };

            structure.Info = membersLists;
            return structure;
        }
    }

    internal class StringAsciiComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return String.Compare(x, y, StringComparison.Ordinal);
        }
    }
}
